using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ASI.Basecode.WebApp.Pages
{
    [AllowAnonymous]
    public class ServerErrorModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
