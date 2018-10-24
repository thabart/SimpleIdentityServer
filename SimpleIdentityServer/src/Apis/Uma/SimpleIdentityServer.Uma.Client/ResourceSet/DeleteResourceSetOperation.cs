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
using SimpleIdentityServer.Common.Client;
using SimpleIdentityServer.Common.Client.Factories;
using SimpleIdentityServer.Common.Dtos.Responses;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Client.ResourceSet
{
    public interface IDeleteResourceSetOperation
    {
        Task<BaseResponse> ExecuteAsync(
            string resourceSetId,
            string resourceSetUrl,
            string authorizationHeaderValue);
    }

    internal class DeleteResourceSetOperation : IDeleteResourceSetOperation
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public DeleteResourceSetOperation(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<BaseResponse> ExecuteAsync(
            string resourceSetId,
            string resourceSetUrl,
            string authorizationHeaderValue)
        {
            if (string.IsNullOrWhiteSpace(resourceSetId))
            {
                throw new ArgumentNullException(nameof(resourceSetId));
            }

            if (string.IsNullOrWhiteSpace(resourceSetUrl))
            {
                throw new ArgumentNullException(nameof(resourceSetUrl));
            }

            if (string.IsNullOrWhiteSpace(authorizationHeaderValue))
            {
                throw new ArgumentNullException(nameof(authorizationHeaderValue));
            }

            if (resourceSetUrl.EndsWith("/"))
            {
                resourceSetUrl = resourceSetUrl.Remove(0, resourceSetUrl.Length - 1);
            }

            resourceSetUrl = resourceSetUrl + "/" + resourceSetId;
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri(resourceSetUrl)
            };
            request.Headers.Add("Authorization", "Bearer " + authorizationHeaderValue);
            var httpClient = _httpClientFactory.GetHttpClient();
            var httpResult = await httpClient.SendAsync(request).ConfigureAwait(false);
            var content = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            try
            {
                httpResult.EnsureSuccessStatusCode();
            }
            catch (Exception)
            {
                return new BaseResponse
                {
                    ContainsError = true,
                    Error = JsonConvert.DeserializeObject<ErrorResponse>(content),
                    HttpStatus = httpResult.StatusCode
                };
            }

            return new BaseResponse();
        }
    }
}
