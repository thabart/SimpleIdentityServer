using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using SimpleIdentityServer.Api.DTOs.Request;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Protector;
using SimpleIdentityServer.Core.Repositories;

namespace SimpleIdentityServer.Api.Controllers
{
    public class ConsentController : Controller
    {
        private readonly IProtector _protector;

        private readonly IScopeRepository _scopeRepository;

        private readonly IClientRepository _clientRepository;

        private readonly IConsentRepository _consentRepository;

        private readonly IResourceOwnerRepository _resourceOwnerRepository;

        public ConsentController(
            IProtector protector,
            IScopeRepository scopeRepository,
            IClientRepository clientRepository,
            IConsentRepository consentRepository,
            IResourceOwnerRepository resourceOwnerRepository)
        {
            _protector = protector;
            _scopeRepository = scopeRepository;
            _clientRepository = clientRepository;
            _consentRepository = consentRepository;
            _resourceOwnerRepository = resourceOwnerRepository;
        }

        [Authorize]

        public ActionResult Index(string code)
        {
            // TODO : display the user information on the website (base controller ?)
            // var user = Request.GetOwinContext().Authentication.User;
            // var claims = user.Claims;
            // var decodedCode = _encoder.Decode(code);

            var request = _protector.Decrypt<AuthorizationRequest>(code);

            var allowedScopeDescriptions = GetScopes(request.scope)
                .Where(s => !s.IsInternal)
                .Select(s => s.Description)
                .ToList();
            var client = _clientRepository.GetClientById(request.client_id);
            var model = new Models.Consent
            {
                ClientDisplayName = client.DisplayName,
                AllowedScopeDescriptions = allowedScopeDescriptions
            };

            return View(model);
        }

        [Authorize]
        public ActionResult Confirm(string code)
        {
            var request = _protector.Decrypt<AuthorizationRequest>(code);

            // Retrieve the user's consent for a client.
            var user = Request.GetOwinContext().Authentication.User;
            var subject = user.Identity.Name;
            var consents = _consentRepository.GetConsentsForGivenUser(subject);
            var consent = consents.SingleOrDefault(c => c.Client.ClientId == request.client_id);
            if (consent == null)
            {
                consent = CreateConsent(request);
                _consentRepository.InsertConsent(consent);
            }

            var redirectUrl = request.redirect_uri;

                
            return View();
        }

        /// <summary>
        /// Create and return a consent object.
        /// </summary>
        /// <param name="request">Authorization request</param>
        /// <returns>User's consent</returns>
        private Consent CreateConsent(AuthorizationRequest request)
        {
            var user = Request.GetOwinContext().Authentication.User;
            var subject = user.Identity.Name;
            var grantedScopes = GetScopes(request.scope);
            return new Consent
            {
                Client = _clientRepository.GetClientById(request.client_id),
                GrantedScopes = grantedScopes,
                ResourceOwner = _resourceOwnerRepository.GetBySubject(subject)
            };
        }

        /// <summary>
        /// Returns a list of scopes from a concatenate list of scopes separated by whitespaces.
        /// </summary>
        /// <param name="concatenateListOfScopes"></param>
        /// <returns>List of scopes</returns>
        private List<Scope> GetScopes(string concatenateListOfScopes)
        {
            var result = new List<Scope>();
            var scopeNames = concatenateListOfScopes.Split(' ');
            foreach (var scopeName in scopeNames)
            {
                var scope = _scopeRepository.GetScopeByName(scopeName);
                result.Add(scope);
            }

            return result;
        } 
    }
}