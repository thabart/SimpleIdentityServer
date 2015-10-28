using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;

using SimpleIdentityServer.Api.DTOs.Request;
using SimpleIdentityServer.Api.Mappings;
using SimpleIdentityServer.Api.Models;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Protector;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Core.Services;

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;

namespace SimpleIdentityServer.Api.Controllers
{
    public class AuthenticateController : Controller
    {
        private readonly IProtector _protector;

        private readonly IEncoder _encoder;

        private readonly IResourceOwnerService _resourceOwnerService;

        private readonly IConsentRepository _consentRepository;

        private readonly IResourceOwnerRepository _resourceOwnerRepository;

        public AuthenticateController(
            IProtector protector,
            IEncoder encoder,
            IResourceOwnerService resourceOwnerService,
            IConsentRepository consentRepository,
            IResourceOwnerRepository resourceOwnerRepository)
        {
            _protector = protector;
            _encoder = encoder;
            _resourceOwnerService = resourceOwnerService;
            _consentRepository = consentRepository;
            _resourceOwnerRepository = resourceOwnerRepository;
        }

        /// <summary>
        /// Return the authentication view.
        /// 1). Redirect to the consent screen if the user is authenticated && the request doesn't contain a login prompt.
        /// 2). Otherwise redirect to the authentication screen.
        /// </summary>
        /// <param name="code">Encrypted request</param>
        /// <returns>Consent screen or authentication screen</returns>
        [HttpGet]
        public ActionResult Index(string code)
        {
            var decodedCode = _encoder.Decode(code);
            var request = _protector.Decrypt<AuthorizationRequest>(decodedCode);
            var authenticationManager = GetAuthenticationManager();
            var user = authenticationManager.User;
            var userIsAuthenticated = user.Identity.IsAuthenticated;
            var promptParameters = ParserHelper.ParsePromptParameters(request.prompt);

            // 1). Redirect to the consent screen if the user is authenticated and the request doesn't contain a login prompt.
            if (userIsAuthenticated && !promptParameters.Contains(PromptParameter.login))
            {
                return RedirectToAction(
                    "Consent", 
                    "Index", 
                    new
                    {
                        code
                    });
            }

            // 2). Return the authenticate screen
            return View(new Authorize
            {
                Code = code
            });
        }

        /// <summary>
        /// Authenticate local user account.
        /// 1). Return an error if the user is already authenticated.
        /// 2). Redirect to the index action if the user credentials are not correct
        /// 3). If there's no consent which has already been approved by the resource owner, then redirect the user-agent to the consent screen.
        /// 4). Redirect the user-agent to the callback-url and pass the authorization code as parameter.
        /// </summary>
        /// <param name="authorize">User's credentials</param>
        /// <returns>Consent screen or redirect to the Index page.</returns>
        [HttpPost]
        public ActionResult Local(Authorize authorize)
        {
            var code = _encoder.Decode(authorize.Code);
            var request = _protector.Decrypt<AuthorizationRequest>(code);
            var promptParameters = ParserHelper.ParsePromptParameters(request.prompt);

            var authenticationManager = GetAuthenticationManager();
            var user = authenticationManager.User;
            var userIsAuthenticated = user.Identity.IsAuthenticated;
            
            // 1). Return an error if the user is already authenticated.
            if (userIsAuthenticated && !promptParameters.Contains(PromptParameter.login))
            {
                throw new IdentityServerException(
                    ErrorCodes.InvalidRequestCode,
                    ErrorDescriptions.TheUserCannotBeReauthenticated);
            }
            
            var subject = _resourceOwnerService.Authenticate(
                authorize.UserName, 
                authorize.Password);

            // TODO : 2). Redirect to the index action if the user credentials are not correct.
            if (string.IsNullOrEmpty(subject))
            {
                return RedirectToAction("Index", new
                {
                    code = authorize.Code
                });
            }

            var identity = GetIdentity(subject);
            authenticationManager.SignIn(
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTime.UtcNow.AddDays(7)
                }, 
                identity);
            
            var consent = _consentRepository.GetConsentsForGivenUser(subject);

            // 3). If there's no consent which has already been approved by the resource owner, then redirect the user-agent to the consent screen.
            if (consent == null)
            {
                var parameter = request.ToParameter();
                return RedirectToAction("Index", "Consent", new { code });
            }

            // 4). TODO : Redirect the user-agent to the callback-url and pass the authorization code as parameter.
            return View();
        }

        /// <summary>
        /// Get the claims identity
        /// </summary>
        /// <param name="subject">Unique identifier of a user</param>
        /// <returns>Claims identify</returns>
        private ClaimsIdentity GetIdentity(string subject)
        {
            var user = _resourceOwnerRepository.GetBySubject(subject);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName)
            };

            return new ClaimsIdentity(claims, DefaultAuthenticationTypes.ApplicationCookie);
        }

        /// <summary>
        /// Get the authentication manager
        /// </summary>
        /// <returns>Authentication manager</returns>
        private IAuthenticationManager GetAuthenticationManager()
        {
            return Request.GetOwinContext().Authentication;
        }
    }
}