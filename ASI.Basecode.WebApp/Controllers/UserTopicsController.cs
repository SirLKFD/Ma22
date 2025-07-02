using Microsoft.AspNetCore.Mvc;

namespace ASI.Basecode.WebApp.Controllers
{
    public class UserTopicsController : Controller
    {
        public IActionResult UserTopics()
        {
            return View();
        }
    }
}
