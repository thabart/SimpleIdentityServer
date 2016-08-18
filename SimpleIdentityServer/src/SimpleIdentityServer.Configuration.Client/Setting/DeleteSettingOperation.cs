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
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Configuration.Client.Setting
{
    public interface IDeleteSettingOperation
    {
        Task<bool> ExecuteAsync(string key,
            string settingUrl,
            string authorizationHeaderValue);
    }

    internal class DeleteSettingOperation : IDeleteSettingOperation
    {
        private readonly IHttpClientFactory _httpClientFactory;

        #region Constructor

        public DeleteSettingOperation(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        #endregion

        #region Public methods

        public async Task<bool> ExecuteAsync(string key, 
            string settingUrl,
            string authorizationHeaderValue)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (string.IsNullOrWhiteSpace(settingUrl))
            {
                throw new ArgumentNullException(nameof(settingUrl));
            }

            if (string.IsNullOrWhiteSpace(authorizationHeaderValue))
            {
                throw new ArgumentNullException(nameof(authorizationHeaderValue));
            }

            Uri uri = null;
            if (!Uri.TryCreate(settingUrl + "/" + key, UriKind.Absolute, out uri))
            {
                throw new ArgumentException("the uri is not correct");
            }

            var httpClient = _httpClientFactory.GetHttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = uri
            };
            request.Headers.Add("Authorization", "Bearer " + authorizationHeaderValue);
            var httpResult = await httpClient.SendAsync(request).ConfigureAwait(false);
            httpResult.EnsureSuccessStatusCode();
            return true;
        }

        #endregion
    }
}
