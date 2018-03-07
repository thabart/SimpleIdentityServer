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

using Newtonsoft.Json.Linq;
using SimpleIdentityServer.Client.Factories;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Client.Operations
{
    public interface IGetUserInfoOperation
    {
        Task<JObject> ExecuteAsync(Uri userInfoUri, string accessToken, bool inBody = false);
    }

    internal class GetUserInfoOperation : IGetUserInfoOperation
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public GetUserInfoOperation(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<JObject> ExecuteAsync(Uri userInfoUri, string accessToken, bool inBody = false)
        {
            if (userInfoUri == null)
            {
                throw new ArgumentNullException(nameof(userInfoUri));
            }

            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new ArgumentNullException(nameof(accessToken));
            }

            var httpClient = _httpClientFactory.GetHttpClient();
            var request = new HttpRequestMessage
            {
                RequestUri = userInfoUri
            };
            request.Headers.Add("Accept", "application/json");

            if (inBody)
            {
                request.Method = HttpMethod.Post;
                request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    {
                        Constants.GrantedTokenNames.AccessToken, accessToken
                    }
                });
            }
            else
            {
                request.Method = HttpMethod.Get;
                request.Headers.Add("Authorization", $"Bearer {accessToken}");
            }

            var serializedContent = await httpClient.SendAsync(request).ConfigureAwait(false);
            var json = await serializedContent.Content.ReadAsStringAsync();
            return JObject.Parse(json);
        }
    }
}
