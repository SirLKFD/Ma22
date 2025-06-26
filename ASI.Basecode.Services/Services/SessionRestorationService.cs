using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Security.Claims;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.Text;

namespace ASI.Basecode.Services.Services
{
    public class SessionRestorationService : IActionFilter
    {
        private readonly ILogger<SessionRestorationService> _logger;

        public SessionRestorationService(ILogger<SessionRestorationService> logger)
        {
            _logger = logger;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var user = context.HttpContext.User;

            if (user.Identity != null && user.Identity.IsAuthenticated)
            {
                var email = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                // Restore session data for authenticated users if session is missing
                if (!string.IsNullOrEmpty(email))
                {
                    var session = context.HttpContext.Session;
                    
                    // Check if session has user name (using byte array)
                    byte[] userNameBytes;
                    var hasUserName = session.TryGetValue("UserName", out userNameBytes);
                    var userName = hasUserName ? Encoding.UTF8.GetString(userNameBytes) : null;
                    
                    // Only restore session if it's missing
                    if (string.IsNullOrEmpty(userName))
                    {
                        _logger.LogInformation($"Session restoration needed for user: {email}");
                        
                        var userService = context.HttpContext.RequestServices.GetService<IUserService>();
                        if (userService != null)
                        {
                            var account = userService.GetUserByEmail(email);
                            if (account != null)
                            {
                                session.Set("HasSession", Encoding.UTF8.GetBytes("Exist"));
                                session.Set("UserName", Encoding.UTF8.GetBytes($"{account.FirstName} {account.LastName}"));
                                session.Set("UserEmail", Encoding.UTF8.GetBytes(account.EmailId));
                                session.Set("AccountId", BitConverter.GetBytes(account.Id));
                                
                                _logger.LogInformation($"Session restored for user: {account.FirstName} {account.LastName}");
                            }
                            else
                            {
                                _logger.LogWarning($"User not found in database for email: {email}");
                            }
                        }
                        else
                        {
                            _logger.LogError("IUserService not available in DI container");
                        }
                    }
                }
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // No-op
        }
    }
} 