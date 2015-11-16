using SimpleIdentityServer.Core.Repositories;

namespace SimpleIdentityServer.Core.Validators
{
    public interface IGrantedTokenValidator
    {
        bool CheckAccessToken(string accessToken);
    }

    public class GrantedTokenValidator
    {
        private readonly IGrantedTokenRepository _grantedTokenRepository;

        public GrantedTokenValidator(IGrantedTokenRepository grantedTokenRepository)
        {
            _grantedTokenRepository = grantedTokenRepository;
        }

        public bool CheckAccessToken(
            string accessToken)
        {
            var grantedToken = _grantedTokenRepository.GetToken(accessToken);
            if (grantedToken == null)
            {
                return false;
            }


            return true;
        }
    }
}
