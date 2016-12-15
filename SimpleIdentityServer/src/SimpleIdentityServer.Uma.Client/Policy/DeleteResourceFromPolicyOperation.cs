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
using System.Net.Http;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Client.Policy
{
    public interface IDeleteResourceFromPolicyOperation
    {
        Task<bool> ExecuteAsync(string id, string resourceId, string url, string token);
    }

    internal class DeleteResourceFromPolicyOperation : IDeleteResourceFromPolicyOperation
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public DeleteResourceFromPolicyOperation(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<bool> ExecuteAsync(string id, string resourceId, string url, string token)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (string.IsNullOrWhiteSpace(resourceId))
            {
                throw new ArgumentNullException(nameof(resourceId));
            }

            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentNullException(nameof(url));
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentNullException(nameof(token));
            }

            if (url.EndsWith("/"))
            {
                url = url.Remove(0, url.Length - 1);
            }

            url = url + "/" + id + "/resources/" + resourceId;
            var httpClient = _httpClientFactory.GetHttpClient();
            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri(url)
            };
            httpRequest.Headers.Add("Authorization", "Bearer " + token);
            var httpResult = await httpClient.SendAsync(httpRequest).ConfigureAwait(false);
            httpResult.EnsureSuccessStatusCode();
            return true;
        }
    }
}
