using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using SimpleIdentityServer.Api.DTOs.Request;
using SimpleIdentityServer.Api.Helpers;
using SimpleIdentityServer.Api.Mappings;
using SimpleIdentityServer.Api.Models;
using SimpleIdentityServer.Core.Operations.Authorization;
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

        private readonly IAddConsentOperation _addConsentOperation;

        private readonly ISessionManager _sessionManager;

        private readonly IResourceOwnerRepository _resourceOwnerRepository;

        public AuthenticateController(
            IProtector protector,
            IEncoder encoder,
            IResourceOwnerService resourceOwnerService,
            IConsentRepository consentRepository,
            IAddConsentOperation addConsentOperation,
            ISessionManager sessionManager,
            IResourceOwnerRepository resourceOwnerRepository)
        {
            _protector = protector;
            _encoder = encoder;
            _resourceOwnerService = resourceOwnerService;
            _consentRepository = consentRepository;
            _addConsentOperation = addConsentOperation;
            _sessionManager = sessionManager;
            _resourceOwnerRepository = resourceOwnerRepository;
        }

        /// <summary>
        /// Return the authentication view.
        /// 1). Redirect to the consent screen if the user is authenticated && the request doesn't contain a login prompt.
        /// 2). Otherwise redirect to the authentication screen.
        /// </summary>
        /// <param name="code">Encrypted request</param>
        /// <returns>consent screen or authentication screen</returns>
        [HttpGet]
        public ActionResult Index(string code)
        {
            var decodedCode = _encoder.Decode(code);
            var request = _protector.Decrypt<AuthorizationRequest>(decodedCode);
            var authenticationManager = GetAuthenticationManager();
            var user = authenticationManager.User;
            var userIsAuthenticated = user.Identity.IsAuthenticated;
            if (userIsAuthenticated && request.prompt != "login")
            {
                return RedirectToAction(
                    "Consent", 
                    "Index", 
                    new
                    {
                        code = code
                    });
            }

            return View(new Authorize
            {
                Code = code
            });
        }

        /// <summary>
        /// Authenticate local user account.
        /// </summary>
        /// <param name="authorize">Contains user's credentials</param>
        /// <returns>Redirect to the consent screen or returns the same page.</returns>
        [HttpPost]
        public ActionResult Local(Authorize authorize)
        {
            var authenticationManager = GetAuthenticationManager();
            var user = authenticationManager.User;
            
            // TODO : Check the user is not authenticated
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

            var identity = GetIdentity(subject);
            authenticationManager.SignIn(
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTime.UtcNow.AddDays(7)
                }, 
                identity);

            // _sessionManager.StoreSession(subject);
            var consent = _consentRepository.GetConsentsForGivenUser(subject);
            if (consent == null)
            {
                var parameter = request.ToParameter();
                // _addConsentOperation.Execute(parameter, subject);
                // return RedirectToAction("Index", "Consent", new { code = code });
            }

            // TODO : redirect to the callback.

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