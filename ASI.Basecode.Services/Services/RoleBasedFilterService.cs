using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace ASI.Basecode.Services.Services
{
    public class RoleBasedFilterService : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            var user = context.HttpContext.User;

            if (user.Identity != null && user.Identity.IsAuthenticated)
            {
                var role = user.FindFirst(ClaimTypes.Role)?.Value;

                if (!string.IsNullOrEmpty(role))
                {
                    if (role == "0")
                    {
                        context.Result = new RedirectToActionResult("UserMaster", "Admin", null);
                    }
                    else if (role == "1")
                    {
                        context.Result = new RedirectToActionResult("UserDashboard", "User", null);
                    }
                    else
                    {
                        context.Result = new RedirectToActionResult("Login", "Account", null);
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
