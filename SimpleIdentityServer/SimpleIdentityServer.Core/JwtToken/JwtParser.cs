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
using System.Linq;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Factories;
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Core.Jwt.Converter;
using SimpleIdentityServer.Core.Jwt.Encrypt;
using SimpleIdentityServer.Core.Jwt.Signature;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Validators;

namespace SimpleIdentityServer.Core.JwtToken
{
    public interface IJwtParser
    {
        bool IsJweToken(string jwe);

        bool IsJwsToken(string jws);

        string Decrypt(
            string jwe,
            string clientId);

        JwsPayload UnSign(
            string jws,
            string clientId);
    }

    public class JwtParser : IJwtParser
    {
        private readonly IJweParser _jweParser;

        private readonly IJwsParser _jwsParser;
        
        private readonly IHttpClientFactory _httpClientFactory;

        private readonly IClientValidator _clientValidator;

        private readonly IJsonWebKeyConverter _jsonWebKeyConverter;

        public JwtParser(
            IJweParser jweParser,
            IJwsParser jwsParser,
            IHttpClientFactory httpClientFactory,
            IClientValidator clientValidator,
            IJsonWebKeyConverter jsonWebKeyConverter)
        {
            _jweParser = jweParser;
            _jwsParser = jwsParser;
            _httpClientFactory = httpClientFactory;
            _clientValidator = clientValidator;
            _jsonWebKeyConverter = jsonWebKeyConverter;
        }

        public bool IsJweToken(string jwe)
        {
            return _jweParser.GetHeader(jwe) != null;
        }

        public bool IsJwsToken(string jws)
        {
            return _jwsParser.GetHeader(jws) != null;
        }

        public string Decrypt(
            string jwe,
            string clientId)
        {
            if (string.IsNullOrWhiteSpace(jwe))
            {
                throw new ArgumentNullException("jwe");
            }

            if (string.IsNullOrWhiteSpace(clientId))
            {
                throw new ArgumentNullException("clientId");
            }

            var client = _clientValidator.ValidateClientExist(clientId);
            if (client == null)
            {
                throw new InvalidOperationException(string.Format(ErrorDescriptions.ClientIsNotValid, clientId));
            }

            const string emptyResult = null;
            var protectedHeader = _jweParser.GetHeader(jwe);
            if (protectedHeader == null)
            {
                return emptyResult;
            }

            var jsonWebKey = GetJsonWebKeyFromClient(client,
                protectedHeader.Kid);
            if (jsonWebKey == null)
            {
                return emptyResult;
            }

            return _jweParser.Parse(jwe,jsonWebKey);
        }

        public JwsPayload UnSign(
            string jws,
            string clientId)
        {
            if (string.IsNullOrWhiteSpace(jws))
            {
                throw new ArgumentNullException("jws");
            }

            if (string.IsNullOrWhiteSpace(clientId))
            {
                throw new ArgumentNullException("clientId");
            }

            var client = _clientValidator.ValidateClientExist(clientId);
            if (client == null)
            {
                throw new InvalidOperationException(string.Format(ErrorDescriptions.ClientIsNotValid, clientId));
            }

            const JwsPayload emptyResult = null;
            var protectedHeader = _jwsParser.GetHeader(jws);
            if (protectedHeader == null)
            {
                return emptyResult;
            }

            var jsonWebKey = GetJsonWebKeyFromClient(client,
                protectedHeader.Kid);
            if (jsonWebKey == null 
                && protectedHeader.Alg != Jwt.Constants.JwsAlgNames.NONE)
            {
                return emptyResult;
            }
            
            if (protectedHeader.Alg == Jwt.Constants.JwsAlgNames.NONE)
            {
                return _jwsParser.GetPayload(jws);
            }

            return _jwsParser.ValidateSignature(jws, jsonWebKey);
        }
        
        private JsonWebKey GetJsonWebKeyFromClient(Client client,
            string kid)
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
                    var json = request.Content.ReadAsStringAsync().Result;
                    var jsonWebKeys = _jsonWebKeyConverter.ConvertFromJson(json);
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
    }
}
