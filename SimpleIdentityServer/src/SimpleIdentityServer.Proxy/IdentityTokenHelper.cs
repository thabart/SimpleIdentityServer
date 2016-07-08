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

using Microsoft.Extensions.DependencyInjection;
using SimpleIdentityServer.Client;
using SimpleIdentityServer.Core.Common.Extensions;
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Core.Jwt.Converter;
using SimpleIdentityServer.Core.Jwt.Encrypt;
using SimpleIdentityServer.Core.Jwt.Signature;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Proxy
{
    public interface IIdentityTokenHelper 
    {
        Task<string> DecryptIdentityTokenByResolution(
               string jwe,
               string configurationUrl);

        Task<string> DecryptIdentityToken(
            string jwe,
            string jwks);

        Task<JwsPayload> UnSignByResolution(
            string jws,
            string configurationUrl);

        Task<JwsPayload> UnSign(
            string jws,
            string jwks);
    }

    public class IdentityTokenHelper : IIdentityTokenHelper
    {
        #region Fields

        private readonly IJweParser _jweParser;

        private readonly IJwsParser _jwsParser;

        private readonly IJsonWebKeyConverter _jsonWebKeyConverter;

        private readonly IDiscoveryClient _discoveryClient;

        #endregion

        #region Constructor

        public IdentityTokenHelper()
        {
            var services = new ServiceCollection();
            RegisterDependencies(services);
            var serviceProvider = services.BuildServiceProvider();
            _jweParser = (IJweParser)serviceProvider.GetService(typeof(IJweParser));
            _jwsParser = (IJwsParser)serviceProvider.GetService(typeof(IJwsParser));
            _jsonWebKeyConverter = (IJsonWebKeyConverter)serviceProvider.GetService(typeof(IJsonWebKeyConverter));
            _discoveryClient = new IdentityServerClientFactory().CreateDiscoveryClient();
        }

        #endregion

        #region Public methods

        public async Task<string> DecryptIdentityTokenByResolution(
            string jwe,
            string configurationUrl)
        {
            if (string.IsNullOrWhiteSpace(configurationUrl))
            {
                throw new ArgumentNullException(nameof(configurationUrl));
            }

            var discoveryInformation = await _discoveryClient.GetDiscoveryInformationAsync(configurationUrl);
            return await DecryptIdentityToken(jwe, discoveryInformation.JwksUri);
        }

        public async Task<string> DecryptIdentityToken(
            string jwe,
            string jwks)
        {
            if (string.IsNullOrWhiteSpace(jwe))
            {
                throw new ArgumentNullException(nameof(jwe));
            }

            if (string.IsNullOrWhiteSpace(jwks))
            {
                throw new ArgumentNullException(nameof(jwks));
            }

            var protectedHeader = _jweParser.GetHeader(jwe);
            if (protectedHeader == null)
            {
                return string.Empty;
            }

            var jsonWebKey = await GetJsonWebKey(jwks, protectedHeader.Kid);
            if (jsonWebKey == null)
            {
                return string.Empty;
            }
            
            return _jweParser.Parse(jwe, jsonWebKey);
        }
        
        public async Task<JwsPayload> UnSignByResolution(
            string jws,
            string configurationUrl)
        {
            if (string.IsNullOrWhiteSpace(configurationUrl))
            {
                throw new ArgumentNullException(nameof(configurationUrl));
            }

            var discoveryInformation = await _discoveryClient.GetDiscoveryInformationAsync(configurationUrl);
            return await UnSign(jws, discoveryInformation.JwksUri);
        }

        public async Task<JwsPayload> UnSign(
            string jws,
            string jwks)
        {
            if (string.IsNullOrWhiteSpace(jws))
            {
                throw new ArgumentNullException(nameof(jws));
            }

            if (string.IsNullOrWhiteSpace(jwks))
            {
                throw new ArgumentNullException(nameof(jwks));
            }

            var protectedHeader = _jwsParser.GetHeader(jws);
            if (protectedHeader == null)
            {
                return null;
            }

            var jsonWebKey = await GetJsonWebKey(jwks, protectedHeader.Kid);
            if (jsonWebKey == null
                && protectedHeader.Alg != Core.Jwt.Constants.JwsAlgNames.NONE)
            {
                return null;
            }

            if (protectedHeader.Alg == Core.Jwt.Constants.JwsAlgNames.NONE)
            {
                return _jwsParser.GetPayload(jws);
            }

            return _jwsParser.ValidateSignature(jws, jsonWebKey);
        }

        #endregion

        #region Private methods

        private async Task<JsonWebKey> GetJsonWebKey(
            string jwksUri,
            string kid)
        {
            JsonWebKey result = null;
            if (!string.IsNullOrWhiteSpace(jwksUri))
            {
                Uri uri = null;
                if (!Uri.TryCreate(jwksUri, UriKind.Absolute, out uri))
                {
                    return null;
                }

                var httpClient = new HttpClient();
                httpClient.BaseAddress = uri;
                var request = await httpClient.GetAsync(uri.AbsoluteUri).ConfigureAwait(false);
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

            return result;
        }

        #endregion

        #region Private static methods

        private static void RegisterDependencies(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSimpleIdentityServerJwt();
        }

        #endregion
    }
}
