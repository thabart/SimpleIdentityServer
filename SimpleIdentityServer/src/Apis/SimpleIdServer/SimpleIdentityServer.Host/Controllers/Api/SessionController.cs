using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using SimpleIdentityServer.Host.Extensions;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Host.Controllers.Api
{
    public class SessionController : Controller
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly AuthenticateOptions _authenticateOptions;

        public SessionController(IAuthenticationService authenticationService,
            AuthenticateOptions authenticateOptions)
        {
            _authenticationService = authenticationService;
            _authenticateOptions = authenticateOptions;
        }

        [HttpGet(Constants.EndPoints.CheckSession)]
        public async Task CheckSession()
        {
            var authenticatedUser = await _authenticationService.GetAuthenticatedUser(this, _authenticateOptions.CookieName);
            if (authenticatedUser == null || !authenticatedUser.Identity.IsAuthenticated)
            {
                return;
            }

            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "SimpleIdentityServer.Host.Views.CheckSession.html";
            string html;
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                using (var reader = new StreamReader(stream))
                {
                    html = reader.ReadToEnd();
                }
            }

            html = html.Replace("{cookieName}", Core.Constants.SESSION_ID);
            Response.ContentType = "text/html; charset=UTF-8";
            var payload = Encoding.UTF8.GetBytes(html);
            await Response.Body.WriteAsync(payload, 0, payload.Length);
        }

        [HttpHead(Constants.EndPoints.EndSession)]
        public async Task RevokeSession()
        {
            // 1. GET THE USER.
            // 2. RETRIEVES THE PARAMETER (id_token_type + post_logout_red + state).
            // 3. DISPLAY VIEW (are-you sure to logout ?)
        }
    }
}
