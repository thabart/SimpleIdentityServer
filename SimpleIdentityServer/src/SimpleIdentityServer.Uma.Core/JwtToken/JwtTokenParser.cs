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
using SimpleIdentityServer.Core.Jwt.Signature;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Uma.Core.JwtToken
{
    public interface IJwtTokenParser
    {

    }

    internal class JwtTokenParser : IJwtTokenParser
    {
        private readonly IJwsParser _jwsParser;

        private readonly IIdentityServerClientFactory _identityServerClientFactory;

        private readonly IParametersProvider _parametersProvider;

        #region Constructor

        public JwtTokenParser(
            IJwsParser jwsParser,
            IIdentityServerClientFactory identityServerClientFactory,
            IParametersProvider parametersProvider)
        {
            _jwsParser = jwsParser;
            _identityServerClientFactory = identityServerClientFactory;
            _parametersProvider = parametersProvider;
        }

        #endregion

        #region Public methods

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
            var keys = jsonWebKeySet.Keys;
            return null;
        }

        #endregion

        #region Private methods

        private List<JsonWebKey> GetJsonWebKeys(List<Dictionary<string, object>> lst)
        {
            var result = new List<JsonWebKey>();
            foreach(var record in lst)
            {
                /*
                 * var use = record[SimpleIdentityServer.Core.Jwt.Constants.JsonWebKeyParameterNames.UseName]
                var jsonWebKey = new JsonWebKey
                {
                    Use = 
                };
                */
            }

            return null;
        }

        #endregion
    }
}
