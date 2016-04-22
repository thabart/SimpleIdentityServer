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
using SimpleIdentityServer.Client.DTOs.Responses;
using SimpleIdentityServer.Client.Factory;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Client.Policy
{
    public interface IGetPoliciesOperation
    {
        Task<List<PolicyResponse>> ExecuteAsync(
            Uri policyUri,
            string authorizationHeaderValue);
    }

    internal class GetPoliciesOperation : IGetPoliciesOperation
    {
        private readonly IHttpClientFactory _httpClientFactory;

        #region Constructor

        public GetPoliciesOperation(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        #endregion

        #region Public methods
        
        public async Task<List<PolicyResponse>> ExecuteAsync(
            Uri policyUri,
            string authorizationHeaderValue)
        {
            if (policyUri == null)
            {
                throw new ArgumentNullException(nameof(policyUri));
            }

            if (string.IsNullOrWhiteSpace(authorizationHeaderValue))
            {
                throw new ArgumentNullException(nameof(authorizationHeaderValue));
            }

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = policyUri
            };
            request.Headers.Add("Authorization", "Bearer " + authorizationHeaderValue);
            var httpClient = _httpClientFactory.GetHttpClient();
            var httpResult = await httpClient.SendAsync(request);
            httpResult.EnsureSuccessStatusCode();
            var content = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonConvert.DeserializeObject<List<PolicyResponse>>(content);
        }

        #endregion
    }
}
