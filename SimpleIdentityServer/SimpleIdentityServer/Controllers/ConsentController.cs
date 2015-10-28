using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using SimpleIdentityServer.Api.DTOs.Request;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Protector;
using SimpleIdentityServer.Core.Repositories;
using System;
using SimpleIdentityServer.Api.Extensions;
using System.Security.Claims;

namespace SimpleIdentityServer.Api.Controllers
{
    [Authorize]
    public class ConsentController : Controller
    {
        private readonly IProtector _protector;

        private readonly IScopeRepository _scopeRepository;

        private readonly IClientRepository _clientRepository;

        private readonly IConsentRepository _consentRepository;

        private readonly IResourceOwnerRepository _resourceOwnerRepository;

        private readonly IAuthorizationCodeRepository _authorizationCodeRepository;

        public ConsentController(
            IProtector protector,
            IScopeRepository scopeRepository,
            IClientRepository clientRepository,
            IConsentRepository consentRepository,
            IResourceOwnerRepository resourceOwnerRepository,
            IAuthorizationCodeRepository authorizationCodeRepository)
        {
            _protector = protector;
            _scopeRepository = scopeRepository;
            _clientRepository = clientRepository;
            _consentRepository = consentRepository;
            _resourceOwnerRepository = resourceOwnerRepository;
            _authorizationCodeRepository = authorizationCodeRepository;
        }


        /// <summary>
        /// Fetch the scopes and client name from the ClientRepository and the encrypted/signed parameter.
        /// Those informations are used to create the consent screen.
        /// </summary>
        /// <param name="code">Encrypted and signed request.</param>
        /// <returns>Consent screen.</returns>
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
                AllowedScopeDescriptions = allowedScopeDescriptions,
                Code = code
            };

            return View(model);
        }
        
        /// <summary>
        /// This method is executed when the user confirm the consent
        /// 1). If there's already consent confirmed in the past by the resource owner
        /// 1).* then we generate an authorization code and redirects to the callback.
        /// 2). If there's no consent then we insert it and the authorization code is returned
       ///  2°.* to the callback url.
        /// </summary>
        /// <param name="code">Crypted and signed request.</param>
        /// <returns>Redirects the authorization code to the callback.</returns>
        public ActionResult Confirm(string code)
        {
            var request = _protector.Decrypt<AuthorizationRequest>(code);
            
            var user = Request.GetOwinContext().Authentication.User;
            var subject = user.FindFirst(ClaimTypes.NameIdentifier).Value;
            var consents = _consentRepository.GetConsentsForGivenUser(subject);
            // 1).
            var consent = consents == null ? null : consents.SingleOrDefault(c => c.Client.ClientId == request.client_id);
            if (consent == null)
            {
                // 2).
                consent = CreateConsent(request);
                _consentRepository.InsertConsent(consent);
            }
            
            var authorizationCode = new AuthorizationCode
            {
                CreateDateTime = DateTime.UtcNow,
                Consent = consent,
                Value = Guid.NewGuid().ToString() 
            };
            _authorizationCodeRepository.AddAuthorizationCode(authorizationCode);

            var redirectUrl = new Uri(request.redirect_uri);
            var redirectUrlWithAuthCode = redirectUrl.AddParameter("code", authorizationCode.Value);
            return Redirect(redirectUrlWithAuthCode.ToString());
        }

        /// <summary>
        /// Action executed when the user refuse the consent.
        /// It redirects to the callback without passing the authorization code in parameter.
        /// </summary>
        /// <param name="code">Encrypted & signed authorization request</param>
        /// <returns>Redirect to the callback url.</returns>
        public ActionResult Cancel(string code)
        {
            var request = _protector.Decrypt<AuthorizationRequest>(code);
            return Redirect(request.redirect_uri);
        }

        /// <summary>
        /// Create and return a consent object.
        /// </summary>
        /// <param name="request">Authorization request</param>
        /// <returns>User's consent</returns>
        private Consent CreateConsent(AuthorizationRequest request)
        {
            var user = Request.GetOwinContext().Authentication.User;
            var subject = user.FindFirst(ClaimTypes.NameIdentifier).Value;
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