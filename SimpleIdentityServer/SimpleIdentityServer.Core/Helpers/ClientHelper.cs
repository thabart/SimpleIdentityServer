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
using SimpleIdentityServer.Core.Validators;

namespace SimpleIdentityServer.Core.Helpers
{
    public interface IClientHelper
    {
        string GenerateIdToken(
            string clientId,
            JwsPayload jwsPayload);
    }

    public sealed class ClientHelper : IClientHelper
    {
        private readonly IClientValidator _clientValidator;

        private readonly IJwtGenerator _jwtGenerator;

        public ClientHelper(
            IClientValidator clientValidator,
            IJwtGenerator jwtGenerator)
        {
            _clientValidator = clientValidator;
            _jwtGenerator = jwtGenerator;
        }

        public string GenerateIdToken(
            string clientId,
            JwsPayload jwsPayload)
        {
            if (string.IsNullOrWhiteSpace(clientId))
            {
                throw new ArgumentNullException("clientId");
            }

            if (jwsPayload == null)
            {
                throw new ArgumentNullException("jwsPayload");
            }

            var client = _clientValidator.ValidateClientExist(clientId);
            var signedResponseAlg = client.GetIdTokenSignedResponseAlg();
            var encryptResponseAlg = client.GetIdTokenEncryptedResponseAlg();
            var encryptResponseEnc = client.GetIdTokenEncryptedResponseEnc();
            if (signedResponseAlg == null)
            {
                signedResponseAlg = JwsAlg.RS256;
            }

            var idToken = _jwtGenerator.Sign(jwsPayload,
                signedResponseAlg.Value);

            if (encryptResponseAlg == null)
            {
                return idToken;
            }

            if (encryptResponseEnc == null)
            {
                encryptResponseEnc = JweEnc.A128CBC_HS256;
            }

            return _jwtGenerator.Encrypt(idToken,
                encryptResponseAlg.Value,
                encryptResponseEnc.Value);
        }
    }
}
