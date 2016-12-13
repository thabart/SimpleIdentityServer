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

using SimpleIdentityServer.Client;
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Core.Jwt.Converter;
using SimpleIdentityServer.Core.Jwt.Signature;
using System;
using System.Linq;
using System.Threading.Tasks;
using Jwt = SimpleIdentityServer.Core.Jwt;

namespace SimpleIdentityServer.Uma.Core.JwtToken
{
    public interface IJwtTokenParser
    {
        Task<JwsPayload> UnSign(string jws);
    }

    internal class JwtTokenParser : IJwtTokenParser
    {
        private readonly IJwsParser _jwsParser;
        private readonly IIdentityServerClientFactory _identityServerClientFactory;
        private readonly IParametersProvider _parametersProvider;
        private readonly IJsonWebKeyConverter _jsonWebKeyConverter;

        public JwtTokenParser(
            IJwsParser jwsParser,
            IIdentityServerClientFactory identityServerClientFactory,
            IParametersProvider parametersProvider,
            IJsonWebKeyConverter jsonWebKeyConverter)
        {
            _jwsParser = jwsParser;
            _identityServerClientFactory = identityServerClientFactory;
            _parametersProvider = parametersProvider;
            _jsonWebKeyConverter = jsonWebKeyConverter;
        }

        public async Task<JwsPayload> UnSign(string jws)
        {
            if (string.IsNullOrWhiteSpace(jws))
            {
                throw new ArgumentNullException(nameof(jws));
            }

            var protectedHeader = _jwsParser.GetHeader(jws);
            if (protectedHeader == null)
            {
                return null;
            }

            var jsonWebKeySet = await _identityServerClientFactory.CreateJwksClient()
                .ResolveAsync(_parametersProvider.GetOpenIdConfigurationUrl())
                .ConfigureAwait(false);
            var jsonWebKeys = _jsonWebKeyConverter.ExtractSerializedKeys(jsonWebKeySet);
            if (jsonWebKeys == null ||
                !jsonWebKeys.Any(j => j.Kid == protectedHeader.Kid))
            {
                return null;
            }

            var jsonWebKey = jsonWebKeys.First(j => j.Kid == protectedHeader.Kid);
            if (protectedHeader.Alg == Jwt.Constants.JwsAlgNames.NONE)
            {
                return _jwsParser.GetPayload(jws);
            }

            return _jwsParser.ValidateSignature(jws, jsonWebKey);
        }
    }
}
