using System.Web.Mvc;

namespace SimpleIdentityServer.Api.Controllers
{
    public class AuthorizeController : Controller
    {
        // GET: Authorize
        public ActionResult Index()
        {
            return View();
        }
    }
}