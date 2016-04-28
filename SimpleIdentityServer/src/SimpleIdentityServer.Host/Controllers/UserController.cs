using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using SimpleIdentityServer.Core.WebSite.User;
using SimpleIdentityServer.Host.Extensions;
using SimpleIdentityServer.Host.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace SimpleIdentityServer.Api.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly IUserActions _userActions;

        #region Constructor

        public UserController(IUserActions userActions)
        {
            _userActions = userActions;
        }

        #endregion

        #region Public methods

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Consent()
        {
            var authenticatedUser = this.GetAuthenticatedUser();
            var consents = _userActions.GetConsents(authenticatedUser);
            var result = new List<ConsentViewModel>();
            foreach (var consent in consents)
            {
                var client = consent.Client;
                var scopes = consent.GrantedScopes;
                var claims = consent.Claims;
                var viewModel = new ConsentViewModel
                {
                    ClientDisplayName = client == null ? string.Empty : client.ClientName,
                    AllowedScopeDescriptions = scopes == null || !scopes.Any() ?
                        new List<string>() :
                        scopes.Select(g => g.Description).ToList(),
                    AllowedIndividualClaims = claims == null ? new List<string>() : claims,
                    LogoUri = client == null ? string.Empty : client.LogoUri,
                    PolicyUri = client == null ? string.Empty : client.PolicyUri,
                    TosUri = client == null ? string.Empty : client.TosUri
                };

                result.Add(viewModel);
            }

            return View(result);
        }

        #endregion
    }
}