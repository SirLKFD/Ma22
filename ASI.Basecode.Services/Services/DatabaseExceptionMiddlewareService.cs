using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace ASI.Basecode.WebApp.Middleware
{
    public class DatabaseExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<DatabaseExceptionMiddleware> _logger;

        public DatabaseExceptionMiddleware(RequestDelegate next, ILogger<DatabaseExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL Server error occurred: {Message}", ex.Message);
                
                // Redirect to the server error page
                context.Response.Redirect("/Home/ServerError");
            }
            catch (Exception ex) when (IsSqlConnectionException(ex))
            {
                _logger.LogError(ex, "SQL Server connection error occurred: {Message}", ex.Message);
                
                // Redirect to the server error page
                context.Response.Redirect("/Home/ServerError");
            }
        }

        private bool IsSqlConnectionException(Exception ex)
        {
            // Check if the exception is related to SQL Server connectivity
            return ex.Message.Contains("A network-related or instance-specific error") ||
                   ex.Message.Contains("The server was not found or was not accessible") ||
                   ex.Message.Contains("Cannot open database") ||
                   (ex.InnerException != null && IsSqlConnectionException(ex.InnerException));
        }
    }
}