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
using SimpleIdentityServer.Client.DTOs.Request;
using SimpleIdentityServer.Client.DTOs.Response;
using SimpleIdentityServer.Client.Factories;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Client.Operations
{
    public interface IPostTokenOperation
    {
        Task<GrantedToken> ExecuteAsync(TokenRequest tokenRequest, string authorizationValue);
    }

    internal class PostTokenOperation : IPostTokenOperation
    {
        private readonly IHttpClientFactory _httpClientFactory;

        #region Constructor

        public PostTokenOperation(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        #endregion

        #region Public methods

        public async Task<GrantedToken> ExecuteAsync(TokenRequest tokenRequest, string authorizationValue)
        {
            if (tokenRequest == null)
            {
                throw new ArgumentNullException(nameof(tokenRequest));
            }

            var httpClient = _httpClientFactory.GetHttpClient();
            var serializedTokenRequest = JsonConvert.SerializeObject(tokenRequest);
            var content = new StringContent(serializedTokenRequest, Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = content
            };
            var result = await httpClient.SendAsync(request);
            return null;
        }

        #endregion
    }
}
