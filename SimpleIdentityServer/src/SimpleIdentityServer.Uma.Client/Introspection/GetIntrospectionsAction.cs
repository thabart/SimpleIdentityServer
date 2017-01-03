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
using SimpleIdentityServer.Uma.Client.Factory;
using SimpleIdentityServer.Uma.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Client.Introspection
{
    public interface IGetIntrospectionsAction
    {
        Task<IEnumerable<IntrospectionResponse>> ExecuteAsync(PostIntrospection parameter, string url);
    }

    internal class GetIntrospectionsAction : IGetIntrospectionsAction
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public GetIntrospectionsAction(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IEnumerable<IntrospectionResponse>> ExecuteAsync(PostIntrospection parameter, string url)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentNullException(nameof(url));
            }

            if (url.EndsWith("/"))
            {
                url = url.Remove(0, url.Length - 1);
            }

            url = url + "/bulk";
            var serializedParameter = JsonConvert.SerializeObject(parameter);
            var body = new StringContent(serializedParameter, Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(url),
                Content = body
            };
            var httpClient = _httpClientFactory.GetHttpClient();
            var httpResult = await httpClient.SendAsync(request);
            httpResult.EnsureSuccessStatusCode();
            var content = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonConvert.DeserializeObject<IEnumerable<IntrospectionResponse>>(content);
        }
    }
}
