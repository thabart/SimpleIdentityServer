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
using SimpleIdentityServer.Scim.Client.Extensions;
using SimpleIdentityServer.Scim.Client.Factories;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Scim.Client
{

    public interface IGroupsClient
    {
        RequestBuilder AddGroup(string baseUrl, string accessToken = null);
        RequestBuilder AddGroup(Uri baseUri, string accessToken = null);
        Task<ScimResponse> GetGroup(string baseUrl, string id, string accessToken = null);
        Task<ScimResponse> GetGroup(Uri baseUri, string id, string accessToken = null);
        Task<ScimResponse> DeleteGroup(string baseUrl, string id, string accessToken = null);
        Task<ScimResponse> DeleteGroup(Uri baseUri, string id, string accessToken = null);
        RequestBuilder UpdateGroup(string baseUrl, string id, string accessToken = null);
        RequestBuilder UpdateGroup(Uri baseUri, string id, string accessToken = null);
        PatchRequestBuilder PartialUpdateGroup(string baseUrl, string id, string accessToken = null);
        PatchRequestBuilder PartialUpdateGroup(Uri baseUri, string id, string accessToken = null);
        Task<ScimResponse> SearchGroups(string baseUrl, SearchParameter parameter, string accessToken = null);
        Task<ScimResponse> SearchGroups(Uri baseUri, SearchParameter parameter, string accessToken = null);
    }

    internal class GroupsClient : IGroupsClient
    {
        private readonly string _schema = Common.Constants.SchemaUrns.Group;
        private readonly IHttpClientFactory _httpClientFactory;

        public GroupsClient(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public RequestBuilder AddGroup(string baseUrl, string accessToken = null)
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                throw new ArgumentNullException(nameof(baseUrl));
            }

            return AddGroup(baseUrl.ParseUri(), accessToken);
        }

        public RequestBuilder AddGroup(Uri baseUri, string accessToken = null)
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

            return new RequestBuilder(_schema, (obj) => AddGroup(obj, new Uri(url), accessToken));
        }

        public async Task<ScimResponse> GetGroup(string baseUrl, string id, string accessToken = null)
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                throw new ArgumentNullException(nameof(baseUrl));
            }

            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            return await GetGroup(baseUrl.ParseUri(), id, accessToken);
        }

        public async Task<ScimResponse> GetGroup(Uri baseUri, string id, string accessToken = null)
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
            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                request.Headers.Add("Authorization", "Bearer " + accessToken);
            }

            return await ParseHttpResponse(await client.SendAsync(request).ConfigureAwait(false));
        }

        public async Task<ScimResponse> DeleteGroup(string baseUrl, string id, string accessToken = null)
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                throw new ArgumentNullException(nameof(baseUrl));
            }

            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            return await DeleteGroup(baseUrl.ParseUri(), id, accessToken);
        }

        public async Task<ScimResponse> DeleteGroup(Uri baseUri, string id, string accessToken = null)
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
                Method = HttpMethod.Delete,
                RequestUri = new Uri(url)
            };
            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                request.Headers.Add("Authorization", "Bearer " + accessToken);
            }

            return await ParseHttpResponse(await client.SendAsync(request).ConfigureAwait(false));
        }

        public RequestBuilder UpdateGroup(string baseUrl, string id, string accessToken = null)
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                throw new ArgumentNullException(nameof(baseUrl));
            }

            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            return UpdateGroup(baseUrl.ParseUri(), id, accessToken);
        }

        public RequestBuilder UpdateGroup(Uri baseUri, string id, string accessToken = null)
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
            return new RequestBuilder(_schema, (obj) => UpdateGroup(obj, new Uri(url), accessToken));
        }

        public PatchRequestBuilder PartialUpdateGroup(string baseUrl, string id, string accessToken)
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                throw new ArgumentNullException(nameof(baseUrl));
            }

            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            return PartialUpdateGroup(baseUrl.ParseUri(), id, accessToken);
        }

        public PatchRequestBuilder PartialUpdateGroup(Uri baseUri, string id, string accessToken = null)
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
            return new PatchRequestBuilder((obj) => PartialUpdateGroup(obj, new Uri(url), accessToken));
        }

        public async Task<ScimResponse> SearchGroups(string baseUrl, SearchParameter parameter, string accessToken = null)
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                throw new ArgumentNullException(nameof(baseUrl));
            }

            return await SearchGroups(baseUrl.ParseUri(), parameter, accessToken);
        }

        public async Task<ScimResponse> SearchGroups(Uri baseUri, SearchParameter parameter, string accessToken = null)
        {
            if (baseUri == null)
            {
                throw new ArgumentNullException(nameof(baseUri));
            }

            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            var url = $"{FormatUrl(baseUri.AbsoluteUri)}/.search";
            var client = _httpClientFactory.GetHttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(url),
                Content = new StringContent(parameter.ToJson())
            };
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                request.Headers.Add("Authorization", "Bearer " + accessToken);
            }

            return await ParseHttpResponse(await client.SendAsync(request).ConfigureAwait(false));
        }

        private async Task<ScimResponse> AddGroup(JObject jObj, Uri uri, string accessToken = null)
        {
            return await ExecuteRequest(jObj, uri, HttpMethod.Post, accessToken);
        }

        private async Task<ScimResponse> UpdateGroup(JObject jObj, Uri uri, string accessToken = null)
        {
            return await ExecuteRequest(jObj, uri, HttpMethod.Put, accessToken);
        }

        private async Task<ScimResponse> PartialUpdateGroup(JObject jObj, Uri uri, string accessToken = null)
        {
            return await ExecuteRequest(jObj, uri, new HttpMethod("PATCH"), accessToken);
        }

        private async Task<ScimResponse> ExecuteRequest(JObject jObj, Uri uri, HttpMethod method, string accessToken = null)
        {
            var request = new HttpRequestMessage
            {
                Method = method,
                RequestUri = uri,
                Content = new StringContent(jObj.ToString())
            };
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                request.Headers.Add("Authorization", "Bearer " + accessToken);
            }

            var client = _httpClientFactory.GetHttpClient();
            return await ParseHttpResponse(await client.SendAsync(request).ConfigureAwait(false));
        }
        
        private static string FormatUrl(string baseUrl)
        {
            var result = baseUrl.ParseUri();
            if (result == null)
            {
                return null;
            }
            
            return baseUrl.TrimEnd('/', '\\') + "/Groups";
        }

        private static async Task<ScimResponse> ParseHttpResponse(HttpResponseMessage response)
        {
            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var result = new ScimResponse
            {
                StatusCode = response.StatusCode
            };
            if (!string.IsNullOrWhiteSpace(json))
            {
                result.Content = JObject.Parse(json);
            }

            return result;
        }
    }
}
