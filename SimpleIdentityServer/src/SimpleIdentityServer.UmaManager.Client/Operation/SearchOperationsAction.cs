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
using SimpleIdentityServer.UmaManager.Client.DTOs.Responses;
using SimpleIdentityServer.UmaManager.Client.Factory;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SimpleIdentityServer.UmaManager.Client.Operation
{
    public interface ISearchOperationsAction 
    {
        Task<SearchOperationResponse> ExecuteAsync(
            string resourceSetId,
            Uri operationUri);
    }

    internal class SearchOperationsAction : ISearchOperationsAction
    {
        private readonly IHttpClientFactory _httpClientFactory;

        #region Constructor

        public SearchOperationsAction(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        #endregion

        #region Public methods

        public async Task<SearchOperationResponse> ExecuteAsync(
            string resourceSetId,
            Uri operationUri)
        {
            if (string.IsNullOrWhiteSpace(resourceSetId))
            {
                throw new ArgumentNullException(nameof(resourceSetId));
            }

            if (operationUri == null)
            {
                throw new ArgumentNullException(nameof(operationUri));
            }

            var operationUrl = $"{operationUri.AbsoluteUri.TrimEnd('/')}/search?resourceSet={resourceSetId}";
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(operationUrl)
            };
            var httpClient = _httpClientFactory.GetHttpClient();
            var httpResult = await httpClient.SendAsync(request);
            httpResult.EnsureSuccessStatusCode();
            var content = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonConvert.DeserializeObject<SearchOperationResponse>(content);
        }

        #endregion
    }
}
