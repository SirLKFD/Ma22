using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASI.Basecode.WebApp.Controllers
{
    public class HomeController : Controller
    {
        [AllowAnonymous]
        public IActionResult LandingPage()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
                switch (role)
                {
                    case "0": // Admin
                        return RedirectToAction("AdminTrainingCategory", "AdminTrainingCategory");
                    case "1": // User
                        return RedirectToAction("UserDashboard", "User");
                    case "2": // SuperAdmin
                        return RedirectToAction("UserMaster", "Admin");
                    default:
                        return RedirectToAction("Login", "Account");
                }
            }
            return View("LandingPage");
        }
    }
}
