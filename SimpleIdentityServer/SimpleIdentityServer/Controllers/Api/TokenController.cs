using System.Web.Http;

using SimpleIdentityServer.Api.DTOs.Request;
using SimpleIdentityServer.Api.Extensions;
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
            switch (tokenRequest.grant_type)
            {
                case GrantTypeRequest.password:
                    var resourceOwnerParameter = tokenRequest.ToResourceOwnerGrantTypeParameter();
                    result = _tokenActions.GetTokenByResourceOwnerCredentialsGrantType(resourceOwnerParameter);
                    break;
                case GrantTypeRequest.authorization_code:
                    var authCodeParameter = tokenRequest.ToAuthorizationCodeGrantTypeParameter();
                    result = _tokenActions.GetTokenByAuthorizationCodeGrantType(authCodeParameter);
                    break;
            }

            return result;
        }
    }
}
