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
using SimpleIdentityServer.Scim.Client.Builders;
using SimpleIdentityServer.Scim.Client.Factories;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Scim.Client.Groups
{
    public interface IGroupsClient
    {
        AddRequestBuilder AddGroup(string baseUrl);
        AddRequestBuilder AddGroup(Uri baseUri);
        Task<ScimResponse> GetGroup(string baseUrl, string id);
        Task<ScimResponse> GetGroup(Uri baseUri, string id);
    }

    internal class GroupsClient : IGroupsClient
    {
        private readonly string _schema = Common.Constants.SchemaUrns.Group;
        private readonly IHttpClientFactory _httpClientFactory;

        public GroupsClient(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public AddRequestBuilder AddGroup(string baseUrl)
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                throw new ArgumentNullException(nameof(baseUrl));
            }

            return AddGroup(ParseUri(baseUrl));
        }

        public AddRequestBuilder AddGroup(Uri baseUri)
        {
            if (baseUri == null)
            {
                throw new ArgumentNullException(nameof(baseUri));
            }

            var url = FormatUrl(baseUri.AbsoluteUri);
            if (url == null)
            {
                throw new ArgumentException($"{baseUri} is not a valid uri");
            }

            return new AddRequestBuilder(_schema, (obj) => AddGroup(obj, new Uri(url)));
        }

        public async Task<ScimResponse> GetGroup(string baseUrl, string id)
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                throw new ArgumentNullException(nameof(baseUrl));
            }

            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            return await GetGroup(ParseUri(baseUrl), id);
        }

        public async Task<ScimResponse> GetGroup(Uri baseUri, string id)
        {
            if (baseUri == null)
            {
                throw new ArgumentNullException(nameof(baseUri));
            }

            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            var url = $"{FormatUrl(baseUri.AbsoluteUri)}/{id}";
            var client = _httpClientFactory.GetHttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(url)
            };
            return await ParseHttpResponse(await client.SendAsync(request).ConfigureAwait(false));
        }

        private async Task<ScimResponse> AddGroup(JObject jObj, Uri uri)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = uri,
                Content = new StringContent(jObj.ToString())
            };
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var client = _httpClientFactory.GetHttpClient();
            return await ParseHttpResponse(await client.SendAsync(request).ConfigureAwait(false));
        }
        
        private static string FormatUrl(string baseUrl)
        {
            var result = ParseUri(baseUrl);
            if (result == null)
            {
                return null;
            }
            
            return baseUrl.TrimEnd('/', '\\') + "/Groups";
        }

        private static Uri ParseUri(string baseUrl)
        {
            Uri result;
            if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out result))
            {
                return null;
            }

            return result;
        }

        private static async Task<ScimResponse> ParseHttpResponse(HttpResponseMessage response)
        {
            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return new ScimResponse
            {
                StatusCode = response.StatusCode,
                Content = JObject.Parse(json)
            };
        }
    }
}
