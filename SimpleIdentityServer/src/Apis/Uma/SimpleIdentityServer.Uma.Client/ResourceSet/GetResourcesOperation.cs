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
using SimpleIdentityServer.Common.Client.Factories;
using SimpleIdentityServer.Common.Dtos.Responses;
using SimpleIdentityServer.Uma.Client.Results;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Client.ResourceSet
{
    public interface IGetResourcesOperation
    {
        Task<GetResourcesResult> ExecuteAsync(string resourceSetUrl, string authorizationHeaderValue);
    }

    internal class GetResourcesOperation : IGetResourcesOperation
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public GetResourcesOperation(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<GetResourcesResult> ExecuteAsync(string resourceSetUrl, string authorizationHeaderValue)
        {
            if (string.IsNullOrWhiteSpace(resourceSetUrl))
            {
                throw new ArgumentNullException(nameof(resourceSetUrl));
            }

            if (string.IsNullOrWhiteSpace(authorizationHeaderValue))
            {
                throw new ArgumentNullException(nameof(authorizationHeaderValue));
            }
            
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(resourceSetUrl)
            };
            request.Headers.Add("Authorization", "Bearer " + authorizationHeaderValue);
            var httpClient = _httpClientFactory.GetHttpClient();
            var httpResult = await httpClient.SendAsync(request).ConfigureAwait(false);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            try
            {
                httpResult.EnsureSuccessStatusCode();
            }
            catch (Exception)
            {
                return new GetResourcesResult
                {
                    ContainsError = true,
                    Error = JsonConvert.DeserializeObject<ErrorResponse>(json),
                    HttpStatus = httpResult.StatusCode
                };
            }

            return new GetResourcesResult
            {
                Content = JsonConvert.DeserializeObject<IEnumerable<string>>(json)
            };
        }
    }
}
