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
        
        [RateLimitationFilter(RateLimitationElementName = "PostToken")]
        [Microsoft.AspNet.Mvc.HttpPost]
        public GrantedToken Post(TokenRequest tokenRequest)
        {
            GrantedToken result = null;
            StringValues authorizationHeader;
            if (!Request.Headers.TryGetValue("Authorization", out authorizationHeader)) 
            {
                throw new System.InvalidOperationException("The resource owner cannot be authenticated");
            }
            
            var authenticationHeaderValue = new AuthenticationHeaderValue("default", authorizationHeader.First());
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
