using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using SimpleIdentityServer.Core.Common;
using SimpleIdentityServer.Core.Common.DTOs;
using SimpleIdentityServer.Core.Common.Repositories;
using SimpleIdentityServer.Core.Common.Serializers;
using SimpleIdentityServer.Core.JwtToken;
using SimpleIdentityServer.Host.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Host.Controllers.Api
{
    public class SessionController : Controller
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly AuthenticateOptions _authenticateOptions;
        private readonly IClientRepository _clientRepository;
        private readonly IJwtParser _jwtParser;

        public SessionController(IAuthenticationService authenticationService,
            AuthenticateOptions authenticateOptions, IClientRepository clientRepository,
            IJwtParser jwtParser)
        {
            _authenticationService = authenticationService;
            _authenticateOptions = authenticateOptions;
            _clientRepository = clientRepository;
            _jwtParser = jwtParser;
        }

        [HttpGet(Constants.EndPoints.CheckSession)]
        public async Task CheckSession()
        {
            await this.DisplayInternalHtml("SimpleIdentityServer.Host.Views.CheckSession.html", (html) =>
            {
                return html.Replace("{cookieName}", Core.Constants.SESSION_ID);
            });
        }

        [HttpGet(Constants.EndPoints.EndSession)]
        public async Task RevokeSession()
        {
            var authenticatedUser = await _authenticationService.GetAuthenticatedUser(this, _authenticateOptions.CookieName);
            if (authenticatedUser == null || !authenticatedUser.Identity.IsAuthenticated)
            {
                await this.DisplayInternalHtml("SimpleIdentityServer.Host.Views.UserNotConnected.html");
                return;
            }

            var url = Constants.EndPoints.EndSessionCallback;
            if (Request.QueryString.HasValue)
            {
                url = $"{url}{Request.QueryString.Value}";
            }

            await this.DisplayInternalHtml("SimpleIdentityServer.Host.Views.RevokeSession.html", (html) =>
            {
                return html.Replace("{endSessionCallbackUrl}", url);
            });
        }

        [HttpGet(Constants.EndPoints.EndSessionCallback)]
        public async Task RevokeSessionCallback()
        {
            var authenticatedUser = await _authenticationService.GetAuthenticatedUser(this, _authenticateOptions.CookieName);
            if (authenticatedUser == null || !authenticatedUser.Identity.IsAuthenticated)
            {
                await this.DisplayInternalHtml("SimpleIdentityServer.Host.Views.UserNotConnected.html");
                return;
            }

            var query = Request.Query;
            var serializer = new ParamSerializer();
            RevokeSessionRequest request = null;
            if (query != null)
            {
                request = serializer.Deserialize<RevokeSessionRequest>(query);
            }
            
            Response.Cookies.Delete(Core.Constants.SESSION_ID);
            await _authenticationService.SignOutAsync(HttpContext, _authenticateOptions.CookieName, new Microsoft.AspNetCore.Authentication.AuthenticationProperties());
            if (request != null && !string.IsNullOrWhiteSpace(request.PostLogoutRedirectUri) && !string.IsNullOrWhiteSpace(request.IdTokenHint))
            {
                var jws = await _jwtParser.UnSignAsync(request.IdTokenHint);
                if (jws != null)
                {
                    var claim = jws.FirstOrDefault(c => c.Key == StandardClaimNames.Azp);
                    if (!claim.Equals(default(KeyValuePair<string, object>)) && claim.Value != null)
                    {
                        var client = await _clientRepository.GetClientByIdAsync(claim.Value.ToString());
                        if (client != null)
                        {
                            if (client.PostLogoutRedirectUris != null && client.PostLogoutRedirectUris.Contains(request.PostLogoutRedirectUri))
                            {
                                var redirectUrl = request.PostLogoutRedirectUri;
                                if (!string.IsNullOrWhiteSpace(request.State))
                                {
                                    redirectUrl = $"{redirectUrl}?state={request.State}";
                                }

                                Response.Redirect(redirectUrl);
                                return;
                            }
                        }
                    }
                }
            }

            await this.DisplayInternalHtml("SimpleIdentityServer.Host.Views.RevokeSessionCallback.html");
        }
    }
}
