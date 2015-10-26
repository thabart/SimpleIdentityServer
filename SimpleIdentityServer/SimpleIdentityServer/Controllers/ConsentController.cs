using SimpleIdentityServer.Api.Helpers;
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

        public ActionResult Index(string code)
        {
            var subject = _sessionManager.GetSession();
            return View();
        }
    }
}