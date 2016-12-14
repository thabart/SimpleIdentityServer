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
using SimpleIdentityServer.Client.DTOs.Requests;
using SimpleIdentityServer.Client.DTOs.Responses;
using SimpleIdentityServer.Uma.Client.Factory;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Client.Authorization
{
    public interface IGetAuthorizationOperation
    {
        Task<AuthorizationResponse> ExecuteAsync(
               PostAuthorization postAuthorization,
               Uri requestUri,
               string authorizationValue);
    }

    internal class GetAuthorizationOperation : IGetAuthorizationOperation
    {
        private readonly IHttpClientFactory _httpClientFactory;
        
        public GetAuthorizationOperation(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<AuthorizationResponse> ExecuteAsync(
            PostAuthorization postAuthorization,
            Uri requestUri,
            string authorizationValue)
        {
            if (postAuthorization == null)
            {
                throw new ArgumentNullException(nameof(postAuthorization));
            }

            if (requestUri == null)
            {
                throw new ArgumentNullException(nameof(requestUri));
            }

            if (string.IsNullOrWhiteSpace(authorizationValue))
            {
                throw new ArgumentNullException(nameof(authorizationValue));
            }

            var httpClient = _httpClientFactory.GetHttpClient();
            var serializedPostAuthorization = JsonConvert.SerializeObject(postAuthorization);
            var body = new StringContent(serializedPostAuthorization, Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = body,
                RequestUri = requestUri
            };
            request.Headers.Add("Authorization", "Bearer " + authorizationValue);
            var result = await httpClient.SendAsync(request).ConfigureAwait(false);
            result.EnsureSuccessStatusCode();
            var content = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonConvert.DeserializeObject<AuthorizationResponse>(content);
        }
    }
}
