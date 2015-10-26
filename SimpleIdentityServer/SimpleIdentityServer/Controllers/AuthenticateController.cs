using System.Web.Mvc;

using SimpleIdentityServer.Api.DTOs.Request;
using SimpleIdentityServer.Api.Models;
using SimpleIdentityServer.Core.Protector;
using SimpleIdentityServer.Core.Services;
using System.Web;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Api.Mappings;
using SimpleIdentityServer.Core.Operations.Authorization;
using SimpleIdentityServer.Api.Helpers;

namespace SimpleIdentityServer.Api.Controllers
{
    public class AuthenticateController : Controller
    {
        private readonly IProtector _protector;

        private readonly IEncoder _encoder;

        private readonly IResourceOwnerService _resourceOwnerService;

        private readonly IConsentRepository _consentRepository;

        private readonly IAddConsentOperation _addConsentOperation;

        private readonly ISessionManager _sessionManager;

        public AuthenticateController(
            IProtector protector,
            IEncoder encoder,
            IResourceOwnerService resourceOwnerService,
            IConsentRepository consentRepository,
            IAddConsentOperation addConsentOperation,
            ISessionManager sessionManager)
        {
            _protector = protector;
            _encoder = encoder;
            _resourceOwnerService = resourceOwnerService;
            _consentRepository = consentRepository;
            _addConsentOperation = addConsentOperation;
            _sessionManager = sessionManager;
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
        public ActionResult Local(Authorize authorize)
        {
            // TODO : Check the user is not authenticated.

            var code = _encoder.Decode(authorize.Code);
            var request = _protector.Decrypt<AuthorizationRequest>(code);
            var subject = _resourceOwnerService.Authenticate(
                authorize.UserName, 
                authorize.Password);
            // Redirect to the index page if the authentication is not ok.
            if (string.IsNullOrEmpty(subject))
            {
                return RedirectToAction("Index", new { code = authorize.Code } );
            }

            _sessionManager.StoreSession(subject);
            var consent = _consentRepository.GetConsentsForGivenUser(subject);
            if (consent == null)
            {
                var parameter = request.ToParameter();
                // _addConsentOperation.Execute(parameter, subject);
                return RedirectToAction("Index", "Consent", new { code = code });
            }

            // TODO : redirect to the callback.

            return View();
        }
    }
}