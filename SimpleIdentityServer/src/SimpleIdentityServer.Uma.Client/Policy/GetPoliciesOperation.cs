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
using SimpleIdentityServer.Uma.Client.Factory;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Client.Policy
{
    public interface IGetPoliciesOperation
    {
        Task<IEnumerable<string>> ExecuteAsync(string url, string token);
    }

    internal class GetPoliciesOperation : IGetPoliciesOperation
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public GetPoliciesOperation(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IEnumerable<string>> ExecuteAsync(string url, string token)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentNullException(nameof(url));
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentNullException(nameof(token));
            }

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(url)
            };
            request.Headers.Add("Authorization", "Bearer " + token);
            var httpClient = _httpClientFactory.GetHttpClient();
            var httpResult = await httpClient.SendAsync(request);
            httpResult.EnsureSuccessStatusCode();
            var content = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonConvert.DeserializeObject<IEnumerable<string>>(content);
        }
    }
}
