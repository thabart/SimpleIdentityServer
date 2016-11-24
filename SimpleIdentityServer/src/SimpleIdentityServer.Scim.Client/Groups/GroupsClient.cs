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
        AddRequestBuilder AddGroup(string url);
    }

    internal class GroupsClient : IGroupsClient
    {
        private readonly string _schema = Common.Constants.SchemaUrns.Group;
        private readonly IHttpClientFactory _httpClientFactory;

        public GroupsClient(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public AddRequestBuilder AddGroup(string url)
        {
            return new AddRequestBuilder(_schema, (obj) => AddGroup(obj, url));
        }

        private async Task<JObject> AddGroup(JObject jObj, string url)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(url),
                Content = new StringContent(jObj.ToString())
            };
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var client = _httpClientFactory.GetHttpClient();
            var resp = await client.GetAsync("http://localhost:5555/Schemas");
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JObject.Parse(json);
        }
    }
}
