using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityServer4.Startup.Controllers
{
    [Authorize]
    public class ClaimsController : Controller
    {
        [HttpGet]
        public IActionResult Index(string returnUrl)
        {
            return View();
        }
    }
}
