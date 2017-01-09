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
using SimpleIdentityServer.Client.DTOs.Response;
using SimpleIdentityServer.Client.Factories;
using System;
using System.Collections.Generic;
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
            X509Certificate certificate);
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
            if (tokenRequest == null)
            {
                throw new ArgumentNullException(nameof(tokenRequest));
            }

            if (requestUri == null)
            {
                throw new ArgumentNullException(nameof(requestUri));
            }

            var httpClient = _httpClientFactory.GetHttpClient();
            return GetToken(tokenRequest, requestUri, authorizationValue, httpClient);
        }

        public Task<GrantedToken> ExecuteAsync(Dictionary<string, string> tokenRequest, Uri requestUri, string authorizationValue, X509Certificate certificate)
        {
            if (certificate == null)
            {
                throw new ArgumentNullException(nameof(certificate));
            }

            var httpClient = _httpClientFactory.GetHttpClient(certificate);
            return GetToken(tokenRequest, requestUri, authorizationValue, httpClient);
        }

        private async Task<GrantedToken> GetToken(Dictionary<string, string> tokenRequest, Uri requestUri, string authorizationValue, HttpClient httpClient)
        {
            var body = new FormUrlEncodedContent(tokenRequest);
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = body,
                RequestUri = requestUri
            };
            request.Headers.Add("Authorization", "Basic " + authorizationValue);
            var result = await httpClient.SendAsync(request);
            result.EnsureSuccessStatusCode();
            var content = await result.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<GrantedToken>(content);
        }
    }
}
