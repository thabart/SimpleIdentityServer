using Microsoft.AspNetCore.Mvc;

namespace SimpleIdentityServer.ResourceManager.Host.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
