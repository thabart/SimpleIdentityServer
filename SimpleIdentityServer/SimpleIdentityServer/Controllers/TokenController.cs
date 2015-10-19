using System.Web.Http;

using SimpleIdentityServer.Api.DTOs.Request;
using SimpleIdentityServer.Core.Operations;
using SimpleIdentityServer.Core.DataAccess.Models;

using SimpleIdentityServer.Api.Attributes;
using SimpleIdentityServer.RateLimitation.Attributes;
using SimpleIdentityServer.Core.Parameters;

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
                    var parameter = new GetAccessTokenWithResourceOwnerCredentialsParameter
                    {
                        ClientId = tokenRequest.client_id,
                        UserName = tokenRequest.username,
                        Password = tokenRequest.password,
                        Scope = tokenRequest.scope
                    };

                    result = _getTokenByResourceOwnerCredentialsGrantType.Execute(parameter);
                    break;
            }

            return result;
        }
    }
}
