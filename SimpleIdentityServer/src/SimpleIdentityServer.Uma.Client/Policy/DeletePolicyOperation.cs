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

using SimpleIdentityServer.Uma.Client.Factory;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Client.Policy
{
    public interface IDeletePolicyOperation
    {
        Task<bool> ExecuteAsync(
               string policyId,
               string policyUrl,
               string authorizationHeaderValue);
    }

    internal class DeletePolicyOperation : IDeletePolicyOperation
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public DeletePolicyOperation(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<bool> ExecuteAsync(
            string policyId,
            string policyUrl,
            string authorizationHeaderValue)
        {
            if (string.IsNullOrWhiteSpace(policyId))
            {
                throw new ArgumentNullException(nameof(policyId));
            }

            if (string.IsNullOrWhiteSpace(policyUrl))
            {
                throw new ArgumentNullException(nameof(policyUrl));
            }

            if (string.IsNullOrWhiteSpace(authorizationHeaderValue))
            {
                throw new ArgumentNullException(nameof(authorizationHeaderValue));
            }

            if (policyUrl.EndsWith("/"))
            {
                policyUrl = policyUrl.Remove(0, policyUrl.Length - 1);
            }

            policyUrl = policyUrl + "/" + policyId;
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri(policyUrl)
            };
            request.Headers.Add("Authorization", "Bearer " + authorizationHeaderValue);
            var httpClient = _httpClientFactory.GetHttpClient();
            var httpResult = await httpClient.SendAsync(request);
            httpResult.EnsureSuccessStatusCode();
            return httpResult.StatusCode == HttpStatusCode.NoContent;
        }
    }
}
