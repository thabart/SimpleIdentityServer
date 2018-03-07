#region copyright
// Copyright 2016 Habart Thierry
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
using SimpleIdentityServer.Client.Factories;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Client.Operations
{
    public interface IRegisterClientOperation
    {
        Task<Core.Common.DTOs.ClientRegistrationResponse> ExecuteAsync(Core.Common.DTOs.Client client, Uri requestUri, string authorizationValue);
    }

    internal class RegisterClientOperation : IRegisterClientOperation
    {
        private readonly IHttpClientFactory _httpClientFactory;
        
        public RegisterClientOperation(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<Core.Common.DTOs.ClientRegistrationResponse> ExecuteAsync(Core.Common.DTOs.Client client, Uri requestUri, string authorizationValue)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            if (requestUri == null)
            {
                throw new ArgumentNullException(nameof(requestUri));
            }

            var httpClient = _httpClientFactory.GetHttpClient();
            var json = JsonConvert.SerializeObject(client, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = new StringContent(json),
                RequestUri = requestUri
            };
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            request.Headers.Add("Authorization", "Basic " + authorizationValue);
            var result = await httpClient.SendAsync(request);
            result.EnsureSuccessStatusCode();
            var content = await result.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Core.Common.DTOs.ClientRegistrationResponse>(content);
        }
    }
}
