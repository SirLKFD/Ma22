using ASI.Basecode.Data;
using ASI.Basecode.Services.Services;
using ASI.Basecode.WebApp;
using ASI.Basecode.WebApp.Extensions.Configuration;
using ASI.Basecode.WebApp.Middleware;
using ASI.Basecode.WebApp.Models;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.IO;

var appBuilder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    ContentRootPath = Directory.GetCurrentDirectory(),
});

// Load config
appBuilder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

// IIS integration
appBuilder.WebHost.UseIISIntegration();

// Configure Cloudinary
appBuilder.Services.Configure<CloudinarySettings>(appBuilder.Configuration.GetSection("CloudinarySettings"));
appBuilder.Services.AddSingleton(cloudinary =>
{
    var settings = appBuilder.Configuration.GetSection("CloudinarySettings").Get<CloudinarySettings>();
    var account = new Account(settings.CloudName, settings.ApiKey, settings.ApiSecret);
    return new Cloudinary(account);
});

// Logging
appBuilder.Logging
    .AddConfiguration(appBuilder.Configuration.GetLoggingSection())
    .AddConsole()
    .AddDebug();

// Role-based filter service
appBuilder.Services.AddScoped<RoleBasedFilterService>();

// Startup configurer
var configurer = new StartupConfigurer(appBuilder.Configuration);
configurer.ConfigureServices(appBuilder.Services);

// Build app
var app = appBuilder.Build();
configurer.ConfigureApp(app, app.Environment);

// Add Database Exception Middleware
app.UseMiddleware<DatabaseExceptionMiddleware>();

// Routing
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=LandingPage}");
app.MapControllers();
app.MapRazorPages();

// Run
app.Run();
