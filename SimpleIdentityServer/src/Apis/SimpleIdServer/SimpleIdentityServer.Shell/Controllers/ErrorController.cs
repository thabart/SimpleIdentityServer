using Microsoft.AspNetCore.Mvc;

namespace SimpleIdentityServer.Shell.Controllers
{
    [Area("Shell")]
    public class ErrorController : Controller
    {
        public ActionResult Get401()
        {
            return View();
        }
        
        public ActionResult Get404() 
        {
            return View();    
        }

        public ActionResult Get500()
        {
            return View();
        }
    }
}