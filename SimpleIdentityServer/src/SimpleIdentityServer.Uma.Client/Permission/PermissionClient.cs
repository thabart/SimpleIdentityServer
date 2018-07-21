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

using SimpleIdentityServer.Client.Configuration;
using SimpleIdentityServer.Client.Extensions;
using SimpleIdentityServer.Uma.Common.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Client.Permission
{
    public interface IPermissionClient
    {
        Task<AddPermissionResponse> Add(PostPermission request, string url, string token);
        Task<AddPermissionResponse> AddByResolution(PostPermission request, string url, string token);
        Task<AddPermissionsResponse> Add(IEnumerable<PostPermission> request, string url, string token);
        Task<AddPermissionsResponse> AddByResolution(IEnumerable<PostPermission> request, string url, string token);
    }

    internal class PermissionClient : IPermissionClient
    {
        private readonly IAddPermissionsOperation _addPermissionsOperation;
        private readonly IGetConfigurationOperation _getConfigurationOperation;

        public PermissionClient(
            IAddPermissionsOperation addPermissionsOperation,
            IGetConfigurationOperation getConfigurationOperation)
        {
            _addPermissionsOperation = addPermissionsOperation;
            _getConfigurationOperation = getConfigurationOperation;
        }

        public Task<AddPermissionResponse> Add(PostPermission request, string url, string token)
        {
            return _addPermissionsOperation.ExecuteAsync(request, url, token);
        }

        public async Task<AddPermissionResponse> AddByResolution(PostPermission request, string url, string token)
        {
            var configuration = await _getConfigurationOperation.ExecuteAsync(UriHelpers.GetUri(url)).ConfigureAwait(false);
            return await Add(request, configuration.PermissionRegistrationEndPoint, token).ConfigureAwait(false);
        }

        public Task<AddPermissionsResponse> Add(IEnumerable<PostPermission> request, string url, string token)
        {
            return _addPermissionsOperation.ExecuteAsync(request, url, token);
        }

        public async Task<AddPermissionsResponse> AddByResolution(IEnumerable<PostPermission> request, string url, string token)
        {
            var configuration = await _getConfigurationOperation.ExecuteAsync(UriHelpers.GetUri(url)).ConfigureAwait(false);
            return await Add(request, configuration.PermissionRegistrationEndPoint, token).ConfigureAwait(false);
        }
    }
}
