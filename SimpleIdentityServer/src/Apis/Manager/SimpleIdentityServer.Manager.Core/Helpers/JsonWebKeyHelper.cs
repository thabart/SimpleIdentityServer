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

using SimpleIdentityServer.Core.Common;
using SimpleIdentityServer.Core.Common.DTOs;
using SimpleIdentityServer.Core.Common.Extensions;
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Core.Jwt.Converter;
using SimpleIdentityServer.Manager.Core.Errors;
using SimpleIdentityServer.Manager.Core.Exceptions;
using SimpleIdentityServer.Manager.Core.Factories;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Manager.Core.Helpers
{
    public interface IJsonWebKeyHelper
    {
        Task<JsonWebKey> GetJsonWebKey(string kid, Uri uri);
    }

    public class JsonWebKeyHelper : IJsonWebKeyHelper
    {
        private readonly IJsonWebKeyConverter _jsonWebKeyConverter;
        private readonly IHttpClientFactory _httpClientFactory;

        public JsonWebKeyHelper(
            IJsonWebKeyConverter jsonWebKeyConverter,
            IHttpClientFactory httpClientFactory)
        {
            _jsonWebKeyConverter = jsonWebKeyConverter;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<JsonWebKey> GetJsonWebKey(string kid, Uri uri)
        {
            if (string.IsNullOrWhiteSpace(kid))
            {
                throw new ArgumentNullException(nameof(kid));
            }

            if (uri == null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            try
            {
                var httpClient = _httpClientFactory.GetHttpClient();
                httpClient.BaseAddress = uri;
                var request = await httpClient.GetAsync(uri.AbsoluteUri).ConfigureAwait(false);
                request.EnsureSuccessStatusCode();
                var json = request.Content.ReadAsStringAsync().Result;
                var jsonWebKeySet = json.DeserializeWithJavascript<JsonWebKeySet>();
                var jsonWebKeys = _jsonWebKeyConverter.ExtractSerializedKeys(jsonWebKeySet);
                return jsonWebKeys.FirstOrDefault(j => j.Kid == kid);
            }
            catch (Exception)
            {
                throw new IdentityServerManagerException(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.TheJsonWebKeyCannotBeFound, kid, uri.AbsoluteUri));
            }
        }
    }
}
