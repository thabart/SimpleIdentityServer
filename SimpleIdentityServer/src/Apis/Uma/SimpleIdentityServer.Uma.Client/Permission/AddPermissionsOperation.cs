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
using SimpleIdentityServer.Common.Client.Factories;
using SimpleIdentityServer.Common.Dtos.Responses;
using SimpleIdentityServer.Uma.Client.Results;
using SimpleIdentityServer.Uma.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Client.Permission
{
    public interface IAddPermissionsOperation
    {
        Task<AddPermissionResult> ExecuteAsync(PostPermission request, string url, string token);
        Task<AddPermissionResult> ExecuteAsync(IEnumerable<PostPermission> request, string url, string token);
    }

    internal class AddPermissionsOperation : IAddPermissionsOperation
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AddPermissionsOperation(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<AddPermissionResult> ExecuteAsync(PostPermission request, string url, string token)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (url == null)
            {
                throw new ArgumentNullException(nameof(url));
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentNullException(nameof(token));
            }

            var httpClient = _httpClientFactory.GetHttpClient();
            var serializedPostPermission = JsonConvert.SerializeObject(request);
            var body = new StringContent(serializedPostPermission, Encoding.UTF8, "application/json");
            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = body,
                RequestUri = new Uri(url)
            };
            httpRequest.Headers.Add("Authorization", "Bearer " + token);
            var result = await httpClient.SendAsync(httpRequest).ConfigureAwait(false);
            var content = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
            try
            {
                result.EnsureSuccessStatusCode();
            }
            catch
            {
                return new AddPermissionResult
                {
                    ContainsError = true,
                    HttpStatus = result.StatusCode,
                    Error = JsonConvert.DeserializeObject<ErrorResponse>(content)
                };
            }

            return new AddPermissionResult
            {
                Content = JsonConvert.DeserializeObject<AddPermissionResponse>(content)
            };
        }

        public async Task<AddPermissionResult> ExecuteAsync(IEnumerable<PostPermission> request, string url, string token)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (url == null)
            {
                throw new ArgumentNullException(nameof(url));
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentNullException(nameof(token));
            }

            if (url.EndsWith("/"))
            {
                url = url.Remove(0, url.Length - 1);
            }

            url = url + "/bulk";

            var httpClient = _httpClientFactory.GetHttpClient();
            var serializedPostPermission = JsonConvert.SerializeObject(request);
            var body = new StringContent(serializedPostPermission, Encoding.UTF8, "application/json");
            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = body,
                RequestUri = new Uri(url)
            };
            httpRequest.Headers.Add("Authorization", "Bearer " + token);
            var result = await httpClient.SendAsync(httpRequest).ConfigureAwait(false);
            var content = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
            try
            {
                result.EnsureSuccessStatusCode();
            }
            catch(Exception)
            {
                return new AddPermissionResult
                {
                    ContainsError = true,
                    HttpStatus = result.StatusCode,
                    Error = JsonConvert.DeserializeObject<ErrorResponse>(content)
                };
            }

            return new AddPermissionResult
            {
                Content = JsonConvert.DeserializeObject<AddPermissionResponse>(content)
            };
        }
    }
}
