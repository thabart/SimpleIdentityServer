using System.Collections.Generic;
using System.Linq;

using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Core.Repositories;

namespace SimpleIdentityServer.Core.Api.Jwks.Actions
{
    public interface IGetSetOfPublicKeysUsedToValidateJwsAction
    {
        List<Dictionary<string, object>> Execute();
    }

    public class GetSetOfPublicKeysUsedToValidateJwsAction : IGetSetOfPublicKeysUsedToValidateJwsAction
    {
        private readonly IJsonWebKeyRepository _jsonWebKeyRepository;

        private readonly IJsonWebKeyEnricher _jsonWebKeyEnricher;

        public GetSetOfPublicKeysUsedToValidateJwsAction(
            IJsonWebKeyRepository jsonWebKeyRepository,
            IJsonWebKeyEnricher jsonWebKeyEnricher)
        {
            _jsonWebKeyRepository = jsonWebKeyRepository;
            _jsonWebKeyEnricher = jsonWebKeyEnricher;
        }

        public List<Dictionary<string, object>> Execute()
        {
            var result = new List<Dictionary<string, object>>();
            var jsonWebKeys = _jsonWebKeyRepository.GetAll();
            // Retrieve all the JWK used by the client to check the signature.
            var jsonWebKeysUsedForSignature = jsonWebKeys.Where(jwk => jwk.Use == Use.Sig && jwk.KeyOps.Contains(KeyOperations.Verify));
            foreach (var jsonWebKey in jsonWebKeysUsedForSignature)
            {
                var publicKeyInformation = _jsonWebKeyEnricher.GetPublicKeyInformation(jsonWebKey);
                var jsonWebKeyInformation = _jsonWebKeyEnricher.GetJsonWebKeyInformation(jsonWebKey);
                // jsonWebKeyInformation.Add(Jwt.Constants.JsonWebKeyParameterNames.KeyOperationsName, new List<string> { Jwt.Constants.MappingKeyOperationToName[KeyOperations.Verify] });
                publicKeyInformation.AddRange(jsonWebKeyInformation);
                result.Add(publicKeyInformation);
            }

            return result;
        }
    }
}
