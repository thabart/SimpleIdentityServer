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
using SimpleIdentityServer.Client.Factories;
using SimpleIdentityServer.Core.Common.DTOs;
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Client.Operations
{
    public interface IGetJsonWebKeysOperation
    {
        Task<JsonWebKeySet> ExecuteAsync(Uri jwksUri);
    }

    internal class GetJsonWebKeysOperation : IGetJsonWebKeysOperation
    {
        private readonly IHttpClientFactory _httpClientFactory;

        #region Constructor

        public GetJsonWebKeysOperation(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        #endregion

        #region Public methods

        public async Task<JsonWebKeySet> ExecuteAsync(Uri jwksUri)
        {
            if (jwksUri == null)
            {
                throw new ArgumentNullException(nameof(jwksUri));
            }

            var httpClient = _httpClientFactory.GetHttpClient();
            var serializedContent = await httpClient.GetStringAsync(jwksUri).ConfigureAwait(false);
            return JsonConvert.DeserializeObject<JsonWebKeySet>(serializedContent);
        }

        #endregion
    }
}
