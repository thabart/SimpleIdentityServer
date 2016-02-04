using Microsoft.AspNet.Mvc;

namespace SimpleIdentityServer.Api.Controllers
{
    public class ErrorController : Controller
    {
        public ActionResult Get401() 
        {
            return View();
        }
    }
}