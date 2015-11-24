using System;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Core.Errors;

namespace SimpleIdentityServer.Core.Validators
{
    public interface IGrantedTokenValidator
    {
        bool CheckAccessToken(
            string accessToken, 
            out string messageErrorCode,
            out string messageErrorDescription);
    }

    public class GrantedTokenValidator : IGrantedTokenValidator
    {
        private readonly IGrantedTokenRepository _grantedTokenRepository;

        public GrantedTokenValidator(IGrantedTokenRepository grantedTokenRepository)
        {
            _grantedTokenRepository = grantedTokenRepository;
        }

        public bool CheckAccessToken(
            string accessToken,
            out string messageErrorCode,
            out string messageErrorDescription)
        {
            messageErrorCode = string.Empty;
            messageErrorDescription = string.Empty;
            var grantedToken = _grantedTokenRepository.GetToken(accessToken);
            if (grantedToken == null)
            {
                messageErrorCode = ErrorCodes.InvalidToken;
                messageErrorDescription = ErrorDescriptions.TheAccessTokenIsNotValid;
                return false;
            }

            var expirationDateTime = grantedToken.CreateDateTime.AddSeconds(grantedToken.ExpiresIn);
            var tokenIsExpired = DateTime.UtcNow > expirationDateTime;
            if (tokenIsExpired)
            {
                messageErrorCode = ErrorCodes.InvalidToken;
                messageErrorDescription = ErrorDescriptions.TheAccessTokenIsExpired;
                return false;
            }
                                  
            return true;
        }
    }
}
