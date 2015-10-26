using System.Web.Mvc;

using SimpleIdentityServer.Api.DTOs.Request;
using SimpleIdentityServer.Api.Models;
using SimpleIdentityServer.Core.Protector;

namespace SimpleIdentityServer.Api.Controllers
{
    public class AuthorizeController : Controller
    {
        private readonly IProtector _protector;

        private readonly IEncoder _encoder;

        public AuthorizeController(
            IProtector protector,
            IEncoder encoder)
        {
            _protector = protector;
            _encoder = encoder;
        }

        [HttpGet]
        public ActionResult Index(string code)
        {
            return View(new Authorize
            {
                Code = code
            });
        }

        [HttpPost]
        public ActionResult Add(Authorize authorize)
        {
            var code = _encoder.Decode(authorize.Code);
            var request = _protector.Decrypt<AuthorizationRequest>(code);
            return View();
        }
    }
}