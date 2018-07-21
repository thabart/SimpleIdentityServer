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

using SimpleIdentityServer.UmaManager.Client.DTOs.Requests;
using SimpleIdentityServer.UmaManager.Client.Factory;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace SimpleIdentityServer.UmaManager.Client.Resources
{
    public interface IAddControllerActionOperation
    {
        Task<bool> ExecuteAsync(
            AddControllerActionRequest actionRequest,
            Uri uri,
            string accessToken);
    }

    internal class AddControllerActionOperation : IAddControllerActionOperation
    {
        private readonly IHttpClientFactory _httpClientFactory;

        #region Constructor

        public AddControllerActionOperation(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        #endregion

        #region Public methods

        public async Task<bool> ExecuteAsync(
            AddControllerActionRequest actionRequest,
            Uri uri,
            string accessToken)
        {
            if (actionRequest == null)
            {
                throw new ArgumentNullException(nameof(actionRequest));
            }

            if (uri == null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = uri,
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    {
                        Constants.AddControllerActionRequestNames.Action, actionRequest.Action
                    },
                    {
                        Constants.AddControllerActionRequestNames.Application, actionRequest.Application
                    },
                    {
                        Constants.AddControllerActionRequestNames.Controller, actionRequest.Controller
                    },
                    {
                        Constants.AddControllerActionRequestNames.Version, actionRequest.Version
                    }
                })
            };
            request.Headers.Add("Authorization", "Bearer " + accessToken);
            var httpClient = _httpClientFactory.GetHttpClient();
            var httpResult = await httpClient.SendAsync(request).ConfigureAwait(false);
            httpResult.EnsureSuccessStatusCode();
            return true;
        }

        #endregion
    }
}
