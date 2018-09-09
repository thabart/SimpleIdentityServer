using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using SimpleIdentityServer.Core.Extensions;
using SimpleIdentityServer.Host.Extensions;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Host.Controllers.Website
{
    public class BaseController : Controller
    {
        protected readonly IAuthenticationService _authenticationService;
        protected readonly AuthenticateOptions _authenticateOptions;

        public BaseController(IAuthenticationService authenticationService, AuthenticateOptions authenticateOptions)
        {
            _authenticationService = authenticationService;
            _authenticateOptions = authenticateOptions;
        }

        public async Task<ClaimsPrincipal> SetUser()
        {
            var authenticatedUser = await _authenticationService.GetAuthenticatedUser(this, _authenticateOptions.CookieName).ConfigureAwait(false);
            var isAuthenticed = authenticatedUser != null && authenticatedUser.Identity != null && authenticatedUser.Identity.IsAuthenticated;
            ViewBag.IsAuthenticated = isAuthenticed;
            if (isAuthenticed)
            {
                ViewBag.Name = authenticatedUser.GetName();
            }
            else
            {
                ViewBag.Name = "unknown";
            }

            return authenticatedUser;
        }
    }
}
