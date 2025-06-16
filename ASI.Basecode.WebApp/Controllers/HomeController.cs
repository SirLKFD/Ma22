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
    }
}
