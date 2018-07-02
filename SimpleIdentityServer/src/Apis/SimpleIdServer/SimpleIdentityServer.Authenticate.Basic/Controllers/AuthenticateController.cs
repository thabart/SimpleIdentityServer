using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using SimpleIdentityServer.Core;
using SimpleIdentityServer.Host;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Authenticate.Basic.Controllers
{
    public class AuthenticateController : Controller
    {
        private readonly AuthenticateOptions _authenticateOptions;
        protected readonly IAuthenticationService _authenticationService;

        public AuthenticateController(AuthenticateOptions authenticateOptions, IAuthenticationService authenticationService)
        {
            _authenticateOptions = authenticateOptions;
            _authenticationService = authenticationService;
        }

        public async Task<ActionResult> Logout()
        {
            HttpContext.Response.Cookies.Delete(Core.Constants.SESSION_ID);
            await _authenticationService.SignOutAsync(HttpContext, _authenticateOptions.CookieName, new Microsoft.AspNetCore.Authentication.AuthenticationProperties());
            return RedirectToAction("Index", "Home", new { area = "Shell" });
        }
    }
}
