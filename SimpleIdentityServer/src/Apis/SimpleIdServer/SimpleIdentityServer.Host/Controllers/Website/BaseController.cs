using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using SimpleIdentityServer.Core.Extensions;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.WebSite.User;
using SimpleIdentityServer.Host.Extensions;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Host.Controllers.Website
{
    public class BaseController : Controller
    {
        protected readonly IAuthenticationService _authenticationService;
        private readonly IUserActions _userActions;
        protected readonly AuthenticateOptions _authenticateOptions;

        public BaseController(IAuthenticationService authenticationService, IUserActions userActions,
            AuthenticateOptions authenticateOptions)
        {
            _authenticationService = authenticationService;
            _userActions = userActions;
            _authenticateOptions = authenticateOptions;
        }

        public async Task<KeyValuePair<ClaimsPrincipal, ResourceOwner>> SetUser()
        {
            var authenticatedUser = await _authenticationService.GetAuthenticatedUser(this, _authenticateOptions.CookieName);
            var isAuthenticed = authenticatedUser != null && authenticatedUser.Identity != null && authenticatedUser.Identity.IsAuthenticated;
            ViewBag.IsAuthenticated = isAuthenticed;
            ResourceOwner resourceOwner = null;
            if (isAuthenticed)
            {
                resourceOwner = await _userActions.GetUser(authenticatedUser);
                ViewBag.IsLocalAccount = resourceOwner.IsLocalAccount;
                ViewBag.Name = authenticatedUser.GetName();
            }
            else
            {
                ViewBag.Name = "unknown";
            }

            return new KeyValuePair<ClaimsPrincipal, ResourceOwner>(authenticatedUser, resourceOwner);
        }
    }
}
