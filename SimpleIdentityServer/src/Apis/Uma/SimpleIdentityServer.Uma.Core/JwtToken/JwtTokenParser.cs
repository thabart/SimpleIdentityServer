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

using SimpleIdentityServer.Core.Common;
using SimpleIdentityServer.Core.Jwt.Converter;
using SimpleIdentityServer.Core.Jwt.Signature;
using SimpleIdentityServer.Uma.Core.Models;
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Uma.Core.JwtToken
{
    public interface IJwtTokenParser
    {
        /// <summary>
        /// Unsign the JWS
        /// </summary>
        /// <param name="jws"></param>
        /// <param name="policyRule"></param>
        /// <returns></returns>
        Task<JwsPayload> UnSign(string jws, PolicyRule policyRule);
    }

    internal class JwtTokenParser : IJwtTokenParser
    {
        private readonly IJwsParser _jwsParser;
        private readonly IJsonWebKeyConverter _jsonWebKeyConverter;

        public JwtTokenParser(IJwsParser jwsParser, IJsonWebKeyConverter jsonWebKeyConverter)
        {
            _jwsParser = jwsParser;
            _jsonWebKeyConverter = jsonWebKeyConverter;
        }

        public Task<JwsPayload> UnSign(string jws, PolicyRule policyRule)
        {
            if (string.IsNullOrWhiteSpace(jws))
            {
                throw new ArgumentNullException(nameof(jws));
            }

            if (policyRule == null)
            {
                throw new ArgumentNullException(nameof(policyRule));
            }

            var protectedHeader = _jwsParser.GetHeader(jws);
            if (protectedHeader == null)
            {
                return null;
            }
            
            if (protectedHeader.Alg == SimpleIdentityServer.Core.Jwt.Constants.JwsAlgNames.NONE)
            {
                return Task.FromResult(_jwsParser.GetPayload(jws));
            }

            var jsonWebKey = new JsonWebKey
            {
                Kty = KeyType.RSA,
                Kid = protectedHeader.Kid,
                SerializedKey = policyRule.SerializedCertificate
            };
            return Task.FromResult(_jwsParser.ValidateSignature(jws, jsonWebKey));
        }
    }
}
