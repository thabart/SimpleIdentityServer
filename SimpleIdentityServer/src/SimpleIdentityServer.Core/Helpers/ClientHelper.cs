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

using System;
using SimpleIdentityServer.Core.Extensions;
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Core.JwtToken;
using SimpleIdentityServer.Core.Models;
using System.Threading.Tasks;
using SimpleIdentityServer.Core.Repositories;

namespace SimpleIdentityServer.Core.Helpers
{
    public interface IClientHelper
    {
        Task<string> GenerateIdTokenAsync(string clientId, JwsPayload jwsPayload);
        Task<string> GenerateIdTokenAsync(Client client, JwsPayload jwsPayload);
    }

    public sealed class ClientHelper : IClientHelper
    {
        private readonly IClientRepository _clientRepository;
        private readonly IJwtGenerator _jwtGenerator;

        public ClientHelper(IClientRepository clientRepository, IJwtGenerator jwtGenerator)
        {
            _clientRepository = clientRepository;
            _jwtGenerator = jwtGenerator;
        }

        public async Task<string> GenerateIdTokenAsync(string clientId, JwsPayload jwsPayload)
        {
            if (string.IsNullOrWhiteSpace(clientId))
            {
                throw new ArgumentNullException(nameof(clientId));
            }

            if (jwsPayload == null)
            {
                throw new ArgumentNullException(nameof(jwsPayload));
            }

            var client = await _clientRepository.GetClientByIdAsync(clientId);
            if (client == null)
            {
                return null;
            }

            return await GenerateIdTokenAsync(client, jwsPayload);
        }

        public async Task<string> GenerateIdTokenAsync(Client client, JwsPayload jwsPayload)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            if (jwsPayload == null)
            {
                throw new ArgumentNullException(nameof(jwsPayload));
            }

            var signedResponseAlg = client.GetIdTokenSignedResponseAlg();
            var encryptResponseAlg = client.GetIdTokenEncryptedResponseAlg();
            var encryptResponseEnc = client.GetIdTokenEncryptedResponseEnc();
            if (signedResponseAlg == null)
            {
                signedResponseAlg = JwsAlg.RS256;
            }

            var idToken = await _jwtGenerator.SignAsync(jwsPayload, signedResponseAlg.Value);
            if (encryptResponseAlg == null)
            {
                return idToken;
            }

            if (encryptResponseEnc == null)
            {
                encryptResponseEnc = JweEnc.A128CBC_HS256;
            }

            return await _jwtGenerator.EncryptAsync(idToken, encryptResponseAlg.Value, encryptResponseEnc.Value);
        }
    }
}
