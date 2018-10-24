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

using SimpleIdentityServer.Common.Client.Factories;
using SimpleIdentityServer.Core.Common;
using SimpleIdentityServer.Core.Common.DTOs.Requests;
using SimpleIdentityServer.Core.Common.Extensions;
using SimpleIdentityServer.Core.Common.Repositories;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Jwt.Converter;
using SimpleIdentityServer.Core.Jwt.Encrypt;
using SimpleIdentityServer.Core.Jwt.Signature;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Core.JwtToken
{
    public interface IJwtParser
    {
        bool IsJweToken(string jwe);
        bool IsJwsToken(string jws);
        Task<string> DecryptAsync(string jwe);
        Task<string> DecryptAsync(string jwe, string clientId);
        Task<string> DecryptWithPasswordAsync(string jwe, string clientId, string password);
        Task<JwsPayload> UnSignAsync(string jws);
        Task<JwsPayload> UnSignAsync(string jws, string clientId);
    }

    public class JwtParser : IJwtParser
    {
        private readonly IJweParser _jweParser;
        private readonly IJwsParser _jwsParser;        
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IClientRepository _clientRepository;
        private readonly IJsonWebKeyConverter _jsonWebKeyConverter;
        private readonly IJsonWebKeyRepository _jsonWebKeyRepository;

        public JwtParser(
            IJweParser jweParser,
            IJwsParser jwsParser,
            IHttpClientFactory httpClientFactory,
            IClientRepository clientRepository,
            IJsonWebKeyConverter jsonWebKeyConverter,
            IJsonWebKeyRepository jsonWebKeyRepository)
        {
            _jweParser = jweParser;
            _jwsParser = jwsParser;
            _httpClientFactory = httpClientFactory;
            _clientRepository = clientRepository;
            _jsonWebKeyConverter = jsonWebKeyConverter;
            _jsonWebKeyRepository = jsonWebKeyRepository;
        }

        public bool IsJweToken(string jwe)
        {
            return _jweParser.GetHeader(jwe) != null;
        }

        public bool IsJwsToken(string jws)
        {
            return _jwsParser.GetHeader(jws) != null;
        }

        public async Task<string> DecryptAsync(string jwe)
        {
            if (string.IsNullOrWhiteSpace(jwe))
            {
                throw new ArgumentNullException(nameof(jwe));
            }

            var protectedHeader = _jweParser.GetHeader(jwe);
            if (protectedHeader == null)
            {
                return string.Empty;
            }

            var jsonWebKey = await _jsonWebKeyRepository.GetByKidAsync(protectedHeader.Kid);
            if (jsonWebKey == null)
            {
                return string.Empty;
            }
            
            return _jweParser.Parse(jwe, jsonWebKey);
        }

        public async Task<string> DecryptAsync(string jwe, string clientId)
        {
            var jsonWebKey = await GetJsonWebKeyToDecrypt(jwe, clientId);
            if (jsonWebKey == null)
            {
                return string.Empty;
            }

            return _jweParser.Parse(jwe, jsonWebKey);
        }

        public async Task<string> DecryptWithPasswordAsync(string jwe, string clientId, string password)
        {
            var jsonWebKey = await GetJsonWebKeyToDecrypt(jwe, clientId);
            if (jsonWebKey == null)
            {
                return string.Empty;
            }

            return _jweParser.ParseByUsingSymmetricPassword(jwe, jsonWebKey, password);
        }

        public async Task<JwsPayload> UnSignAsync(string jws)
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

            var jsonWebKey = await _jsonWebKeyRepository.GetByKidAsync(protectedHeader.Kid);
            return UnSignWithJsonWebKey(jsonWebKey, protectedHeader, jws);
        }

        public async Task<JwsPayload> UnSignAsync(string jws, string clientId)
        {
            if (string.IsNullOrWhiteSpace(jws))
            {
                throw new ArgumentNullException(nameof(jws));
            }

            if (string.IsNullOrWhiteSpace(clientId))
            {
                throw new ArgumentNullException(nameof(clientId));
            }

            var client = await _clientRepository.GetClientByIdAsync(clientId);
            if (client == null)
            {
                throw new InvalidOperationException(string.Format(ErrorDescriptions.ClientIsNotValid, clientId));
            }

            var protectedHeader = _jwsParser.GetHeader(jws);
            if (protectedHeader == null)
            {
                return null;
            }

            var jsonWebKey = await GetJsonWebKeyFromClient(client, protectedHeader.Kid);
            return UnSignWithJsonWebKey(jsonWebKey, protectedHeader, jws);
        }

        #region Private methods

        private JwsPayload UnSignWithJsonWebKey(JsonWebKey jsonWebKey, JwsProtectedHeader jwsProtectedHeader, string jws)
        {
            if (jsonWebKey == null
                && jwsProtectedHeader.Alg != Jwt.Constants.JwsAlgNames.NONE)
            {
                return null;
            }

            if (jwsProtectedHeader.Alg == Jwt.Constants.JwsAlgNames.NONE)
            {
                return _jwsParser.GetPayload(jws);
            }

            return _jwsParser.ValidateSignature(jws, jsonWebKey);
        }

        private async Task<JsonWebKey> GetJsonWebKeyToDecrypt(string jwe, string clientId)
        {
            if (string.IsNullOrWhiteSpace(jwe))
            {
                throw new ArgumentNullException(nameof(jwe));
            }

            if (string.IsNullOrWhiteSpace(clientId))
            {
                throw new ArgumentNullException(nameof(clientId));
            }

            var client = await _clientRepository.GetClientByIdAsync(clientId);
            if (client == null)
            {
                throw new InvalidOperationException(string.Format(ErrorDescriptions.ClientIsNotValid, clientId));
            }

            var protectedHeader = _jweParser.GetHeader(jwe);
            if (protectedHeader == null)
            {
                return null;
            }

            var jsonWebKey = await GetJsonWebKeyFromClient(client, protectedHeader.Kid);
            return jsonWebKey;
        }
        
        private async Task<JsonWebKey> GetJsonWebKeyFromClient(Core.Common.Models.Client client, string kid)
        {
            JsonWebKey result = null;
            // Fetch the json web key from the jwks_uri
            if (!string.IsNullOrWhiteSpace(client.JwksUri))
            {
                Uri uri = null;
                if (!Uri.TryCreate(client.JwksUri, UriKind.Absolute, out uri))
                {
                    return null;
                }

                var httpClient = _httpClientFactory.GetHttpClient();
                httpClient.BaseAddress = uri;
                var request = httpClient.GetAsync(uri.AbsoluteUri).Result;
                try
                {
                    request.EnsureSuccessStatusCode();
                    var json = await request.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var jsonWebKeySet = json.DeserializeWithJavascript<JsonWebKeySet>();
                    var jsonWebKeys = _jsonWebKeyConverter.ExtractSerializedKeys(jsonWebKeySet);
                    return jsonWebKeys.FirstOrDefault(j => j.Kid == kid);
                }
                catch (Exception)
                {
                    return null;
                }
            }

            // Fetch the json web key from the jwks
            if (client.JsonWebKeys != null && 
                client.JsonWebKeys.Any())
            {
                result = client.JsonWebKeys.FirstOrDefault(j => j.Kid == kid);
            }

            return result;
        }

        #endregion
    }
}
