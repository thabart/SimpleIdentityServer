using SimpleIdentityServer.Api.Helpers;
using System.Web;
using System.Web.Mvc;

namespace SimpleIdentityServer.Api.Controllers
{
    public class ConsentController : Controller
    {
        private readonly ISessionManager _sessionManager;

        public ConsentController(ISessionManager sessionManager)
        {
            _sessionManager = sessionManager;
        }

        [Authorize]

        public ActionResult Index(string code)
        {
            var u = Request.GetOwinContext().Authentication.User;
            var claims = u.Claims;
            var subject = _sessionManager.GetSession();
            return View();
        }
    }
}