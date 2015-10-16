using System.Web.Http;

using SimpleIdentityServer.Api.DTOs.Request;
using SimpleIdentityServer.Core.Operations;
using SimpleIdentityServer.Core.DataAccess.Models;

using SimpleIdentityServer.Api.Attributes;
using SimpleIdentityServer.RateLimitation.Attributes;

namespace SimpleIdentityServer.Api.Controllers
{
    [RoutePrefix("token")]
    public class TokenController : ApiController
    {
        private readonly IGetTokenByResourceOwnerCredentialsGrantType _getTokenByResourceOwnerCredentialsGrantType;

        public TokenController(
            IGetTokenByResourceOwnerCredentialsGrantType getTokenByResourceOwnerCredentialsGrantType)
        {
            _getTokenByResourceOwnerCredentialsGrantType = getTokenByResourceOwnerCredentialsGrantType;
        }
        
        [SwaggerOperation("PostToken")]
        [RateLimitationFilter(RateLimitationElementName = "PostToken")]
        public GrantedToken Post(TokenRequest tokenRequest)
        {
            GrantedToken result = null;
            switch (tokenRequest.grant_type)
            {
                case GrantTypeRequest.password:
                    result = _getTokenByResourceOwnerCredentialsGrantType.Execute(
                        tokenRequest.username, 
                        tokenRequest.password, 
                        tokenRequest.client_id,
                        tokenRequest.scope);
                    break;
            }

            return result;
        }

        [RateLimitationFilter(RateLimitationElementName = "PostToken")]
        public string Get()
        {
            return "";
        }
    }
}
