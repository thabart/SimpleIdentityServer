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
using SimpleIdentityServer.Client.Factory;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Client.ResourceSet
{
    public interface IAddResourceSetOperation
    {
        Task<bool> ExecuteAsync(
            PostResourceSet postResourceSet,
            Uri resourceSetUri,
            string authorizationHeaderValue);
    }

    public class AddResourceSetOperation : IAddResourceSetOperation
    {
        private readonly IHttpClientFactory _httpClientFactory;

        #region Constructor

        public AddResourceSetOperation(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        #endregion

        #region Public methods

        public async Task<bool> ExecuteAsync(
            PostResourceSet postResourceSet,
            Uri resourceSetUri,
            string authorizationHeaderValue)
        {
            if (postResourceSet == null)
            {
                throw new ArgumentNullException(nameof(postResourceSet));
            }

            if (resourceSetUri == null)
            {
                throw new ArgumentNullException(nameof(resourceSetUri));
            }

            if (string.IsNullOrWhiteSpace(authorizationHeaderValue))
            {
                throw new ArgumentNullException(nameof(authorizationHeaderValue));
            }

            var httpClient = _httpClientFactory.GetHttpClient();
            var serializedPostResourceSet = JsonConvert.SerializeObject(postResourceSet);
            var body = new StringContent(serializedPostResourceSet, Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage
            {
                Content = body,
                Method = HttpMethod.Post,
                RequestUri = resourceSetUri
            };
            request.Headers.Add("Authorization", "Bearer " + authorizationHeaderValue);
            var httpResult = await httpClient.SendAsync(request);
            httpResult.EnsureSuccessStatusCode();
            return httpResult.StatusCode == HttpStatusCode.Created;
        }

        #endregion
    }
}
