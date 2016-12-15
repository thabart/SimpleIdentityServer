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

using SimpleIdentityServer.Client.Configuration;
using SimpleIdentityServer.Client.Extensions;
using SimpleIdentityServer.Uma.Common.DTOs;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Client.Authorization
{
    public interface IAuthorizationClient
    {
        Task<AuthorizationResponse> Get(PostAuthorization request, string url, string token);
        Task<AuthorizationResponse> GetByResolution(PostAuthorization request, string url, string token);
    }

    internal class AuthorizationClient : IAuthorizationClient
    {
        private readonly IGetAuthorizationOperation _getAuthorizationOperation;
        private readonly IGetConfigurationOperation _getConfigurationOperation;

        public AuthorizationClient(
            IGetAuthorizationOperation getAuthorizationOperation,
            IGetConfigurationOperation getConfigurationOperation)
        {
            _getAuthorizationOperation = getAuthorizationOperation;
            _getConfigurationOperation = getConfigurationOperation;
        }

        public Task<AuthorizationResponse> Get(PostAuthorization request, string url, string token)
        {
            return _getAuthorizationOperation.ExecuteAsync(request, url, token);
        }

        public async Task<AuthorizationResponse> GetByResolution(PostAuthorization request, string url, string token)
        {
            var configuration = await _getConfigurationOperation.ExecuteAsync(UriHelpers.GetUri(url));
            return await Get(request, configuration.RptEndPoint, token);
        }
    }
}
