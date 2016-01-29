using System.Web.Http;

using SimpleIdentityServer.Host.DTOs.Request;
using SimpleIdentityServer.Host.Extensions;
using SimpleIdentityServer.Core.Api.Token;

using SimpleIdentityServer.Api.Attributes;
using SimpleIdentityServer.RateLimitation.Attributes;
using SimpleIdentityServer.Core.Models;

namespace SimpleIdentityServer.Api.Controllers.Api
{
    public class TokenController : ApiController
    {
        private readonly ITokenActions _tokenActions;

        public TokenController(
            ITokenActions tokenActions)
        {
            _tokenActions = tokenActions;
        }
        
        [SwaggerOperation("PostToken")]
        [RateLimitationFilter(RateLimitationElementName = "PostToken")]
        public GrantedToken Post(TokenRequest tokenRequest)
        {
            GrantedToken result = null;
            var authorizationHeader = Request.Headers.Authorization;
            switch (tokenRequest.grant_type)
            {
                case GrantTypeRequest.password:
                    var resourceOwnerParameter = tokenRequest.ToResourceOwnerGrantTypeParameter();
                    result = _tokenActions.GetTokenByResourceOwnerCredentialsGrantType(resourceOwnerParameter, authorizationHeader);
                    break;
                case GrantTypeRequest.authorization_code:
                    var authCodeParameter = tokenRequest.ToAuthorizationCodeGrantTypeParameter();
                    result = _tokenActions.GetTokenByAuthorizationCodeGrantType(
                        authCodeParameter,
                        authorizationHeader);
                    break;
                case GrantTypeRequest.refresh_token:
                    var refreshTokenParameter = tokenRequest.ToRefreshTokenGrantTypeParameter();
                    result = _tokenActions.GetTokenByRefreshTokenGrantType(refreshTokenParameter,
                        authorizationHeader);
                    break;
            }

            return result;
        }
    }
}
