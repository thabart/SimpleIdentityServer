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

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SimpleIdentityServer.Client.DTOs.Response;
using SimpleIdentityServer.Client.Factories;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
#if NET
using System.Net;
#endif
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Client.Operations
{
    public interface IPostTokenOperation
    {
        Task<GrantedToken> ExecuteAsync(
            Dictionary<string, string> tokenRequest,
            Uri requestUri,
            string authorizationValue);

        Task<GrantedToken> ExecuteAsync(
            Dictionary<string, string> tokenRequest,
            Uri requestUri,
            string authorizationValue,
            X509Certificate2 certificate);
    }

    internal class PostTokenOperation : IPostTokenOperation
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public PostTokenOperation(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public Task<GrantedToken> ExecuteAsync(
            Dictionary<string, string> tokenRequest,
            Uri requestUri,
            string authorizationValue)
        {
            return ExecuteAsync(tokenRequest, requestUri, authorizationValue, null);
        }

        public async Task<GrantedToken> ExecuteAsync(Dictionary<string, string> tokenRequest, Uri requestUri, string authorizationValue, X509Certificate2 certificate)
        {
            if (tokenRequest == null)
            {
                throw new ArgumentNullException(nameof(tokenRequest));
            }

            if (requestUri == null)
            {
                throw new ArgumentNullException(nameof(requestUri));
            }

            var httpClient = _httpClientFactory.GetHttpClient();
            var body = new FormUrlEncodedContent(tokenRequest);
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = body,
                RequestUri = requestUri
            };
            if (certificate != null)
            {
                var bytes = certificate.RawData;
                var base64Encoded = Convert.ToBase64String(bytes);
                request.Headers.Add("X-ARR-ClientCert", base64Encoded);
            }

            request.Headers.Add("Authorization", "Basic " + authorizationValue);
            var result = await httpClient.SendAsync(request);
            // result.EnsureSuccessStatusCode();
            var content = await result.Content.ReadAsStringAsync();
            var jObj = JObject.Parse(content);
            JToken accessToken = jObj[Constants.GrantedTokenNames.AccessToken],
                expiresIn = jObj[Constants.GrantedTokenNames.ExpiresIn],
                scope = jObj[Constants.GrantedTokenNames.Scope],
                refreshToken = jObj[Constants.GrantedTokenNames.RefreshToken],
                tokenType = jObj[Constants.GrantedTokenNames.TokenType],
                idToken = jObj[Constants.GrantedTokenNames.IdToken];
            var scopes = new List<string>();
            if(scope != null)
            {
                JArray arr;
                if ((arr = scope as JArray) != null)
                {
                    foreach (var rec in arr)
                    {
                        scopes.Add(rec.Value<string>());
                    }
                }
                else
                {
                    scopes = scope.Value<string>().Split(' ').ToList();
                }
            }

            return new GrantedToken
            {
                AccessToken = accessToken != null ? accessToken.Value<string>() : null,
                ExpiresIn = expiresIn != null ? expiresIn.Value<int>() : default(int),
                IdToken = idToken != null ? idToken.Value<string>() : null,
                RefreshToken = refreshToken != null ? refreshToken.Value<string>() : null,
                TokenType = tokenType != null ? tokenType.Value<string>() : null,
                Scope = scopes
            };
        }
    }
}
