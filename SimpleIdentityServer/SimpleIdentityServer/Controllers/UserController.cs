using System.Web.Mvc;

namespace SimpleIdentityServer.Api.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }
    }
}