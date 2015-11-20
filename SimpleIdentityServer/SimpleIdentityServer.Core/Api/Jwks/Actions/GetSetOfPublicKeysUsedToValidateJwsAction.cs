using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using SimpleIdentityServer.Core.Common.Extensions;
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Core.Repositories;

namespace SimpleIdentityServer.Core.Api.Jwks.Actions
{
    public interface IGetSetOfPublicKeysUsedToValidateJwsAction
    {
        List<JsonWebKey> Execute();
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

        public List<JsonWebKey> Execute()
        {
            var jsonWebKeys = _jsonWebKeyRepository.GetAll();
            var jsonWebKeysUsedForSignature = jsonWebKeys.Where(jwk => jwk.Use == Use.Sig);
            
            return null;
        }
    }
}
