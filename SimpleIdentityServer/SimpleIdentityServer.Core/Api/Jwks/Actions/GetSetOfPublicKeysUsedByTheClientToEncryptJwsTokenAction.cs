using System.Collections.Generic;
using System.Linq;

using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Core.Repositories;

namespace SimpleIdentityServer.Core.Api.Jwks.Actions
{
    public interface IGetSetOfPublicKeysUsedByTheClientToEncryptJwsTokenAction
    {
        List<Dictionary<string, object>> Execute();
    }

    public class GetSetOfPublicKeysUsedByTheClientToEncryptJwsTokenAction : IGetSetOfPublicKeysUsedByTheClientToEncryptJwsTokenAction
    {
        private readonly IJsonWebKeyEnricher _jsonWebKeyEnricher;

        private readonly IJsonWebKeyRepository _jsonWebKeyRepository;

        public GetSetOfPublicKeysUsedByTheClientToEncryptJwsTokenAction(
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
            // Retrieve all the JWK used by the client to encrypt the JWS
            var jsonWebKeysUsedForEncryption =
                jsonWebKeys.Where(jwk => jwk.Use == Use.Enc && jwk.KeyOps.Contains(KeyOperations.Encrypt));
            foreach (var jsonWebKey in jsonWebKeysUsedForEncryption)
            {
                var publicKeyInformation = _jsonWebKeyEnricher.GetPublicKeyInformation(jsonWebKey);
                var jsonWebKeyInformation = _jsonWebKeyEnricher.GetJsonWebKeyInformation(jsonWebKey);
                jsonWebKeyInformation.Add(Jwt.Constants.JsonWebKeyParameterNames.KeyOperationsName, new List<string> { Jwt.Constants.MappingKeyOperationToName[KeyOperations.Encrypt] } );
                publicKeyInformation.AddRange(jsonWebKeyInformation);
                result.Add(publicKeyInformation);
            }

            return result;
        }
    }
}
