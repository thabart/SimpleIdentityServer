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
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Core.Jwt.Encrypt;
using SimpleIdentityServer.Core.Jwt.Signature;
using SimpleIdentityServer.Core.Repositories;

namespace SimpleIdentityServer.Core.JwtToken
{
    public interface IJwtParser
    {
        string Decrypt(
               string jwe);

        JwsPayload UnSign(
            string jws);
    }

    public class JwtParser : IJwtParser
    {
        private readonly IJweParser _jweParser;

        private readonly IJwsParser _jwsParser;

        private readonly IClientRepository _clientRepository;

        private readonly IJsonWebKeyRepository _jsonWebKeyRepository;

        public JwtParser(
            IJweParser jweParser,
            IJwsParser jwsParser,
            IJsonWebKeyRepository jsonWebKeyRepository)
        {
            _jweParser = jweParser;
            _jwsParser = jwsParser;
            _jsonWebKeyRepository = jsonWebKeyRepository;
        }

        public string Decrypt(
            string jwe)
        {
            var protectedHeader = _jweParser.GetHeader(jwe);
            if (protectedHeader == null)
            {
                return jwe;
            }

            var jsonWebKey = _jsonWebKeyRepository.GetByKid(protectedHeader.Kid);
            if (jsonWebKey == null)
            {
                return jwe;
            }

            return _jweParser.Parse(jwe,
                jsonWebKey);
        }

        public JwsPayload UnSign(
            string jws)
        {
            var protectedHeader = _jwsParser.GetHeader(jws);
            if (protectedHeader == null)
            {
                return null;
            }

            var jsonWebKey = _jsonWebKeyRepository.GetByKid(protectedHeader.Kid);
            return _jwsParser.ValidateSignature(jws, jsonWebKey);
        }
    }
}
