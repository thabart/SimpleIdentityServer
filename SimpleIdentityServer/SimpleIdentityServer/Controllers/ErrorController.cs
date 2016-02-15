using System.Web.Mvc;

namespace SimpleIdentityServer.Api.Controllers
{
    public class ErrorController : Controller
    {
        public ViewResult Index()
        {
            return View();
        }
    }
}