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
using SimpleIdentityServer.Configuration.Client.DTOs.Responses;
using SimpleIdentityServer.Configuration.Client.Factory;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Configuration.Client.AuthProvider
{
    public interface IGetAuthProviderOperation
    {
        Task<AuthenticationProviderResponse> ExecuteAsync(
            string name,
            string authenticationProviderUrl,
            string authorizationHeaderValue);
    }

    internal class GetAuthProviderOperation : IGetAuthProviderOperation
    {
        private readonly IHttpClientFactory _httpClientFactory;

        #region Constructor

        public GetAuthProviderOperation(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        #endregion

        #region Public methods

        public async Task<AuthenticationProviderResponse> ExecuteAsync(
            string name, 
            string authenticationProviderUrl, 
            string authorizationHeaderValue)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (string.IsNullOrWhiteSpace(authenticationProviderUrl))
            {
                throw new ArgumentNullException(nameof(authenticationProviderUrl));
            }

            if (string.IsNullOrWhiteSpace(authorizationHeaderValue))
            {
                throw new ArgumentNullException(nameof(authorizationHeaderValue));
            }

            Uri uri = null;
            if (!Uri.TryCreate(authenticationProviderUrl + "/" + name, UriKind.Absolute, out uri))
            {
                throw new ArgumentException("the uri is not correct");
            }

            var httpClient = _httpClientFactory.GetHttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = uri
            };
            request.Headers.Add("Authorization", "Bearer " + authorizationHeaderValue);
            var httpResult = await httpClient.SendAsync(request);
            httpResult.EnsureSuccessStatusCode();
            var content = await httpResult.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<AuthenticationProviderResponse>(content);
        }

        #endregion
    }
}
