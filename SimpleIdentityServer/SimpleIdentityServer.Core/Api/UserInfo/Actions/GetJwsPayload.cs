using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Core.JwtToken;
using SimpleIdentityServer.Core.Repositories;
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

        private readonly IGrantedTokenRepository _grantedTokenRepository;

        private readonly IJwtParser _jwtParser;

        public GetJwsPayload(
            IGrantedTokenValidator grantedTokenValidator,
            IGrantedTokenRepository grantedTokenRepository,
            IJwtParser jwtParser)
        {
            _grantedTokenValidator = grantedTokenValidator;
            _grantedTokenRepository = grantedTokenRepository;
            _jwtParser = jwtParser;
        }

        public JwsPayload Execute(string accessToken)
        {
            string messageErrorCode;
            string messageErrorDescription;
            // Check if the access token is still valid otherwise raise an authorization exception.
            if (!_grantedTokenValidator.CheckAccessToken(
                accessToken, 
                out messageErrorCode, 
                out messageErrorDescription))
            {
                throw new AuthorizationException(messageErrorCode, messageErrorDescription);
            }

            var grantedToken = _grantedTokenRepository.GetToken(accessToken);
            return grantedToken.UserInfoPayLoad;
        }
    }
}
