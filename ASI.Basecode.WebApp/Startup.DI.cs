using ASI.Basecode.Data;
using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Repositories;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using ASI.Basecode.Services.Services;
using ASI.Basecode.WebApp.Authentication;
using ASI.Basecode.WebApp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ASI.Basecode.WebApp
{
    // Other services configuration
    internal partial class StartupConfigurer
    {
        /// <summary>
        /// Configures the other services.
        /// </summary>
        private void ConfigureOtherServices()
        {
            // Framework
            this._services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            this._services.TryAddSingleton<IActionContextAccessor, ActionContextAccessor>();

            // Common
            this._services.AddScoped<TokenProvider>();
            this._services.TryAddSingleton<TokenProviderOptionsFactory>();
            this._services.TryAddSingleton<TokenValidationParametersFactory>();
            this._services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Services
            this._services.TryAddSingleton<TokenValidationParametersFactory>();
            this._services.AddScoped<IUserService, UserService>();
            this._services.AddScoped<ITrainingCategoryService, TrainingCategoryService>();
            this._services.AddScoped<ITrainingService, TrainingService>();
            this._services.AddScoped<ITopicService, TopicService>();
            this._services.AddScoped<ITopicMediaService, TopicMediaService>();

            // Repositories
            this._services.AddScoped<IUserRepository, UserRepository>();
            this._services.AddScoped<ITrainingCategoryRepository, TrainingCategoryRepository>();
            this._services.AddScoped<ITrainingRepository, TrainingRepository>();
            this._services.AddScoped<ITopicRepository, TopicRepository>();
            this._services.AddScoped<ITopicMediaRepository, TopicMediaRepository>();
            // Manager Class
            this._services.AddScoped<SignInManager>();

            this._services.AddHttpClient();

            // Role-based filter service
            this._services.AddScoped<RoleBasedFilterService>();
            this._services.AddScoped<SessionRestorationService>();

            // New services
            this._services.AddScoped<IPasswordResetService, PasswordResetService>();
            this._services.AddScoped<IEmailService, EmailService>();
            this._services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();
        }
    }
}
