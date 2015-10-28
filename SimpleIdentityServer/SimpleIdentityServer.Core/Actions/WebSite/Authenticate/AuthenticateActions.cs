using SimpleIdentityServer.Core.Actions.WebSite.Authenticate.Actions;
using SimpleIdentityServer.Core.Protector;

namespace SimpleIdentityServer.Core.Actions.WebSite.Authenticate
{
    public interface IAuthenticateAction
    {

    }

    public class AuthenticateActions : IAuthenticateAction
    {
        private readonly IAuthenticateResourceOwnerAction _authenticateResourceOwnerAction;

        private readonly IProtector _protector;

        public AuthenticateActions(
            IAuthenticateResourceOwnerAction authenticateResourceOwnerAction,
            IProtector protector)
        {
            _authenticateResourceOwnerAction = authenticateResourceOwnerAction;
            _protector = protector;
        }

        public void AuthenticateResourceOwner(string encryptedRequest)
        {
            var request = _protector.Decrypt<AuthorizationRequest>(decodedCode);
        }
    }
}
