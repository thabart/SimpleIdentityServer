using System.Collections.Generic;
using SimpleIdentityServer.Core.Api.Jwks.Actions;
using SimpleIdentityServer.Core.Jwt.Signature;

namespace SimpleIdentityServer.Core.Api.Jwks
{
    public interface IJwksActions
    {
        JsonWebKeySet GetJwks();
    }

    public class JwksActions : IJwksActions
    {
        private readonly IGetSetOfPublicKeysUsedToValidateJwsAction _getSetOfPublicKeysUsedToValidateJwsAction;

        private readonly IGetSetOfPublicKeysUsedByTheClientToEncryptJwsTokenAction
            _getSetOfPublicKeysUsedByTheClientToEncryptJwsTokenAction;

        public JwksActions(
            IGetSetOfPublicKeysUsedToValidateJwsAction getSetOfPublicKeysUsedToValidateJwsAction,
            IGetSetOfPublicKeysUsedByTheClientToEncryptJwsTokenAction getSetOfPublicKeysUsedByTheClientToEncryptJwsTokenAction)
        {
            _getSetOfPublicKeysUsedToValidateJwsAction = getSetOfPublicKeysUsedToValidateJwsAction;
            _getSetOfPublicKeysUsedByTheClientToEncryptJwsTokenAction =
                getSetOfPublicKeysUsedByTheClientToEncryptJwsTokenAction;
        }

        public JsonWebKeySet GetJwks()
        {
            var publicKeysUsedToValidateSignature = _getSetOfPublicKeysUsedToValidateJwsAction.Execute();
            var publicKeysUsedForClientEncryption = _getSetOfPublicKeysUsedByTheClientToEncryptJwsTokenAction.Execute();
            var result = new JsonWebKeySet
            {
                Keys = new List<Dictionary<string, object>>()
            };

            result.Keys.AddRange(publicKeysUsedToValidateSignature);
            result.Keys.AddRange(publicKeysUsedForClientEncryption);
            return result;
        }
    }
}
