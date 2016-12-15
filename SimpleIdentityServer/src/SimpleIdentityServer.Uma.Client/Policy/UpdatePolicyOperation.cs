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
using SimpleIdentityServer.Uma.Client.Factory;
using SimpleIdentityServer.Uma.Common.DTOs;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Client.Policy
{
    public interface IUpdatePolicyOperation
    {
        Task<bool> ExecuteAsync(PutPolicy request, string url, string token);
    }

    internal class UpdatePolicyOperation : IUpdatePolicyOperation
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public UpdatePolicyOperation(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<bool> ExecuteAsync(PutPolicy request, string url, string token)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentNullException(nameof(url));
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentNullException(nameof(token));
            }

            var httpClient = _httpClientFactory.GetHttpClient();
            var serializedPostResourceSet = JsonConvert.SerializeObject(request);
            var body = new StringContent(serializedPostResourceSet, Encoding.UTF8, "application/json");
            var httpRequest = new HttpRequestMessage
            {
                Content = body,
                Method = HttpMethod.Put,
                RequestUri = new Uri(url)
            };
            httpRequest.Headers.Add("Authorization", "Bearer " + token);
            var httpResult = await httpClient.SendAsync(httpRequest).ConfigureAwait(false);
            httpResult.EnsureSuccessStatusCode();
            return true;
        }
    }
}
