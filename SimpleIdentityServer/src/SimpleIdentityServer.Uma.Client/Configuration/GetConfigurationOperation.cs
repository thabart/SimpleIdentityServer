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
using SimpleIdentityServer.Client.DTOs.Responses;
using SimpleIdentityServer.Client.Factory;
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Client.Configuration
{
    public interface IGetConfigurationOperation
    {
        Task<ConfigurationResponse> ExecuteAsync(Uri configurationUri);
    }

    public class GetConfigurationOperation : IGetConfigurationOperation
    {
        private readonly IHttpClientFactory _httpClientFactory;

        #region Constructor

        public GetConfigurationOperation(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        #endregion

        #region Public methods

        public async Task<ConfigurationResponse> ExecuteAsync(Uri configurationUri)
        {
            if (configurationUri == null)
            {
                throw new ArgumentNullException(nameof(configurationUri));
            }

            var httpClient = _httpClientFactory.GetHttpClient();
            var result = await httpClient.GetStringAsync(configurationUri);
            return JsonConvert.DeserializeObject<ConfigurationResponse>(result);
        }

        #endregion
    }
}
