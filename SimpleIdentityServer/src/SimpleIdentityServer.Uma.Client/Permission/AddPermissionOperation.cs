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
using SimpleIdentityServer.Client.Factory;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Client.Permission
{
    public interface IAddPermissionOperation
    {
        Task<AddPermissionResponse> ExecuteAsync(
            PostPermission postPermission,
            Uri requestUri,
            string authorizationValue);
    }

    public class AddPermissionOperation : IAddPermissionOperation
    {
        private readonly IHttpClientFactory _httpClientFactory;

        #region Constructor

        public AddPermissionOperation(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        #endregion

        #region Public methods

        public async Task<AddPermissionResponse> ExecuteAsync(
            PostPermission postPermission,
            Uri requestUri,
            string authorizationValue)
        {
            if (postPermission == null)
            {
                throw new ArgumentNullException(nameof(postPermission));
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
            var serializedPostPermission = JsonConvert.SerializeObject(postPermission);
            var body = new StringContent(serializedPostPermission, Encoding.UTF8, "application/json");
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
            return JsonConvert.DeserializeObject<AddPermissionResponse>(content);
        }

        #endregion
    }
}
