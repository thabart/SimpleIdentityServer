using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Core.Validators;

namespace SimpleIdentityServer.Core.Api.UserInfo.Actions
{
    public interface IGetJwsPayload
    {
        JwsPayload Execute(string accessToken);
    }

    public class GetJwsPayload : IGetJwsPayload
    {
        private readonly IGrantedTokenValidator _grantedTokenValidator;

        public GetJwsPayload(IGrantedTokenValidator grantedTokenValidator)
        {
            _grantedTokenValidator = grantedTokenValidator;
        }

        public JwsPayload Execute(string accessToken)
        {
            if (!_grantedTokenValidator.CheckAccessToken(accessToken))
            {
                // todo : throw the appropriate exception.
            }

            return null;
        }
    }
}
