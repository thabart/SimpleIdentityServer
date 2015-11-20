using System.Collections.Generic;
using SimpleIdentityServer.Core.Api.Jwks.Actions;
using SimpleIdentityServer.Core.Jwt;

namespace SimpleIdentityServer.Core.Api.Jwks
{
    public interface IJwksActions
    {
        List<JsonWebKey> GetSetOfPublicKeysUsedToValidateJws();
    }

    public class JwksActions : IJwksActions
    {
        private readonly IGetSetOfPublicKeysUsedToValidateJwsAction _getSetOfPublicKeysUsedToValidateJwsAction;

        public JwksActions(IGetSetOfPublicKeysUsedToValidateJwsAction getSetOfPublicKeysUsedToValidateJwsAction)
        {
            _getSetOfPublicKeysUsedToValidateJwsAction = getSetOfPublicKeysUsedToValidateJwsAction;
        }

        public List<JsonWebKey> GetSetOfPublicKeysUsedToValidateJws()
        {
            return _getSetOfPublicKeysUsedToValidateJwsAction.Execute();
        }
    }
}
