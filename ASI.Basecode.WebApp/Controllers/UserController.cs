using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASI.Basecode.WebApp.Controllers
{
    public class UserController : Controller
    {
        [Authorize(Roles = "1")]

        public IActionResult UserDashboard()
        {
            return View();
        }
    }
}
