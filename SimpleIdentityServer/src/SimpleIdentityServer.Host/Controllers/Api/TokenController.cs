using SimpleIdentityServer.Host.DTOs.Request;
using SimpleIdentityServer.Host.Extensions;
using SimpleIdentityServer.Core.Api.Token;

using SimpleIdentityServer.RateLimitation.Attributes;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Host;
using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.Primitives;
using System.Linq;
using System.Net.Http.Headers;

namespace SimpleIdentityServer.Api.Controllers.Api
{
    [Microsoft.AspNet.Mvc.Route(Constants.EndPoints.Token)]
    public class TokenController : Controller
    {
        private readonly ITokenActions _tokenActions;

        public TokenController(
            ITokenActions tokenActions)
        {
            _tokenActions = tokenActions;
        }
        
        [TypeFilter(typeof(RateLimitationFilter), Arguments = new object[] { "PostToken" })]
        [HttpPost]
        public GrantedToken Post(TokenRequest tokenRequest)
        {
            GrantedToken result = null;
            StringValues authorizationHeader;
            AuthenticationHeaderValue authenticationHeaderValue = null;
            if (Request.Headers.TryGetValue("Authorization", out authorizationHeader)) 
            {
                var authorizationHeaderValue = authorizationHeader.First();
                var splittedAuthorizationHeaderValue = authorizationHeaderValue.Split(' ');
                if (splittedAuthorizationHeaderValue.Count() == 2)
                {
                    authenticationHeaderValue = new AuthenticationHeaderValue(
                        splittedAuthorizationHeaderValue[0],
                        splittedAuthorizationHeaderValue[1]);
                }
            }
            
            switch (tokenRequest.grant_type)
            {
                case GrantTypeRequest.password:
                    var resourceOwnerParameter = tokenRequest.ToResourceOwnerGrantTypeParameter();
                    result = _tokenActions.GetTokenByResourceOwnerCredentialsGrantType(resourceOwnerParameter, authenticationHeaderValue);
                    break;
                case GrantTypeRequest.authorization_code:
                    var authCodeParameter = tokenRequest.ToAuthorizationCodeGrantTypeParameter();
                    result = _tokenActions.GetTokenByAuthorizationCodeGrantType(
                        authCodeParameter,
                        authenticationHeaderValue);
                    break;
                case GrantTypeRequest.refresh_token:
                    var refreshTokenParameter = tokenRequest.ToRefreshTokenGrantTypeParameter();
                    result = _tokenActions.GetTokenByRefreshTokenGrantType(refreshTokenParameter,
                        authenticationHeaderValue);
                    break;
            }

            return result;
        }
    }
}
