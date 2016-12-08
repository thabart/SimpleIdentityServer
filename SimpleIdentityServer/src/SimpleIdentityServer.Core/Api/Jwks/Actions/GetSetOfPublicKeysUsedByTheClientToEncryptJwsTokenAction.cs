#region copyright
// Copyright 2015 Habart Thierry
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion
using System.Collections.Generic;
using System.Linq;

using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Core.Repositories;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Core.Api.Jwks.Actions
{
    public interface IGetSetOfPublicKeysUsedByTheClientToEncryptJwsTokenAction
    {
        Task<List<Dictionary<string, object>>> Execute();
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

        public async Task<List<Dictionary<string, object>>> Execute()
        {
            var result = new List<Dictionary<string, object>>();
            var jsonWebKeys = await _jsonWebKeyRepository.GetAllAsync();
            // Retrieve all the JWK used by the client to encrypt the JWS
            var jsonWebKeysUsedForEncryption =
                jsonWebKeys.Where(jwk => jwk.Use == Use.Enc && jwk.KeyOps.Contains(KeyOperations.Encrypt));
            foreach (var jsonWebKey in jsonWebKeysUsedForEncryption)
            {
                var publicKeyInformation = _jsonWebKeyEnricher.GetPublicKeyInformation(jsonWebKey);
                var jsonWebKeyInformation = _jsonWebKeyEnricher.GetJsonWebKeyInformation(jsonWebKey);
                // jsonWebKeyInformation.Add(Jwt.Constants.JsonWebKeyParameterNames.KeyOperationsName, new List<string> { Jwt.Constants.MappingKeyOperationToName[KeyOperations.Encrypt] } );
                publicKeyInformation.AddRange(jsonWebKeyInformation);
                result.Add(publicKeyInformation);
            }

            return result;
        }
    }
}
