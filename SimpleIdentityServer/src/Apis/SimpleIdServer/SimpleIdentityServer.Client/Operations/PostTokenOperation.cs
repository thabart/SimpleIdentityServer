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
using SimpleIdentityServer.Client.Results;
using SimpleIdentityServer.Common.Client.Factories;
using SimpleIdentityServer.Core.Common;
using SimpleIdentityServer.Core.Common.DTOs.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Client.Operations
{
    public interface IPostTokenOperation
    {
        Task<GetTokenResult> ExecuteAsync(Dictionary<string, string> tokenRequest, Uri requestUri, string authorizationValue);
        Task<GetTokenResult> ExecuteAsync(Dictionary<string, string> tokenRequest, Uri requestUri, string authorizationValue, X509Certificate2 certificate);
    }

    internal class PostTokenOperation : IPostTokenOperation
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public PostTokenOperation(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public Task<GetTokenResult> ExecuteAsync(Dictionary<string, string> tokenRequest, Uri requestUri, string authorizationValue)
        {
            return ExecuteAsync(tokenRequest, requestUri, authorizationValue, null);
        }

        public async Task<GetTokenResult> ExecuteAsync(Dictionary<string, string> tokenRequest, Uri requestUri, string authorizationValue, X509Certificate2 certificate)
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
            var result = await httpClient.SendAsync(request).ConfigureAwait(false);
            var content = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
            try
            {
                result.EnsureSuccessStatusCode();
            }
            catch(Exception)
            {
                return new GetTokenResult
                {
                    ContainsError = true,
                    Error = JsonConvert.DeserializeObject<ErrorResponseWithState>(content),
                    Status = result.StatusCode
                };
            }

            return new GetTokenResult
            {
                Content = JsonConvert.DeserializeObject<GrantedTokenResponse>(content)
            };
        }
    }
}
