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

using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using SimpleIdentityServer.Client.Factories;
using SimpleIdentityServer.Core.Common.DTOs;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Xml;

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
            var content = await response.Content.ReadAsStringAsync();
            var result = new ApiResult
            {
                StatusCode = response.StatusCode
            };

            if (!string.IsNullOrWhiteSpace(content))
            {
                try
                {
                    if (request.ResponseMode != null && request.ResponseMode == ResponseModes.FormPost)
                    {
                        var source = WebUtility.HtmlDecode(content);
                        var doc  = new HtmlDocument();
                        doc.LoadHtml(source);
                        var jsonObj = new JObject();
                        var inputs = doc.DocumentNode.Descendants().Where(d => d.Name == "input" && d.Attributes != null && d.Attributes.Any(a => a.Name == "type" && a.Value == "hidden"));
                        foreach(var input in inputs)
                        {
                            var nameAttr = input.Attributes.FirstOrDefault(a => a.Name == "name");
                            var valueAttr = input.Attributes.FirstOrDefault(a => a.Name == "value");
                            if (nameAttr == null || valueAttr == null)
                            {
                                continue;
                            }

                            jsonObj.Add(new JProperty(nameAttr.Value, valueAttr.Value));
                        }

                        result.Content = jsonObj;
                    }
                    else
                    {
                        result.Content = JObject.Parse(content);
                    }
                }
                catch
                {
                    // Trace.WriteLine("the content is not a JSON object");
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