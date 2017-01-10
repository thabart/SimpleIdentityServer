#region copyright
// Copyright 2016 Habart Thierry
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
using SimpleIdentityServer.Core.Common.DTOs;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Client.Operations
{
    public interface IGetAuthorizationOperation
    {
        Task<ApiResult> ExecuteAsync(Uri uri, AuthorizationRequest request);
    }

    internal class GetAuthorizationOperation : IGetAuthorizationOperation
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public GetAuthorizationOperation(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<ApiResult> ExecuteAsync(Uri uri, AuthorizationRequest request)
        {
            if (uri == null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var httpClient = _httpClientFactory.GetHttpClient();
            var uriBuilder = new UriBuilder(uri);
            uriBuilder.Query = request.GetQueryString();
            var response = await httpClient.GetAsync(uriBuilder.Uri);
            var json = await response.Content.ReadAsStringAsync();
            var result = new ApiResult
            {
                StatusCode = response.StatusCode
            };

            if (!string.IsNullOrWhiteSpace(json))
            {
                try
                {
                    result.Content = JObject.Parse(json);
                }
                catch
                {
                    Trace.WriteLine("the content is not a JSON object");
                }
            }

            if (response.Headers.Location != null)
            {
                result.Location = response.Headers.Location;
            }

            return result;
        }
    }
}