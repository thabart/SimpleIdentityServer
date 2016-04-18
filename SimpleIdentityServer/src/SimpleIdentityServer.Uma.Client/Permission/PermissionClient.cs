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
using SimpleIdentityServer.Client.DTOs.Requests;
using SimpleIdentityServer.Client.DTOs.Responses;
using SimpleIdentityServer.Client.Errors;
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Client.Permission
{
    public interface IPermissionClient
    {
        Task<AddPermissionResponse> AddPermissionAsync(
            PostPermission postPermission,
            string permissionUrl,
            string authorizationValue);

        Task<AddPermissionResponse> AddPermissionAsync(
            PostPermission postPermission,
            Uri permissionUri,
            string authorizationValue);

        Task<AddPermissionResponse> AddPermissionByResolvingUrlAsync(
            PostPermission postPermission,
            string configurationUrl,
            string authorizationValue);

        Task<AddPermissionResponse> AddPermissionByResolvingUrlAsync(
            PostPermission postPermission,
            Uri configurationUri,
            string authorizationValue);
    }

    internal class PermissionClient : IPermissionClient
    {
        private readonly IAddPermissionOperation _addPermissionOperation;

        private readonly IGetConfigurationOperation _getConfigurationOperation;

        #region Constructor

        public PermissionClient(
            IAddPermissionOperation addPermissionOperation,
            IGetConfigurationOperation getConfigurationOperation)
        {
            _addPermissionOperation = addPermissionOperation;
            _getConfigurationOperation = getConfigurationOperation;
        }

        #endregion

        #region Public methods

        public async Task<AddPermissionResponse> AddPermissionAsync(
            PostPermission postPermission,
            string permissionUrl,
            string authorizationValue)
        {
            if (string.IsNullOrWhiteSpace(permissionUrl))
            {
                throw new ArgumentNullException(nameof(permissionUrl));
            }

            Uri uri = null;
            if (!Uri.TryCreate(permissionUrl, UriKind.Absolute, out uri))
            {
                throw new ArgumentException(string.Format(ErrorDescriptions.TheUriIsNotWellFormed, permissionUrl));
            }

            return await AddPermissionAsync(postPermission, uri, authorizationValue);
        }

        public async Task<AddPermissionResponse> AddPermissionAsync(
            PostPermission postPermission,
            Uri permissionUri,
            string authorizationValue)
        {
            if (postPermission == null)
            {
                throw new ArgumentNullException(nameof(postPermission));
            }

            if (permissionUri == null)
            {
                throw new ArgumentNullException(nameof(permissionUri));
            }

            if (string.IsNullOrWhiteSpace(authorizationValue))
            {
                throw new ArgumentNullException(nameof(authorizationValue));
            }

            return await _addPermissionOperation.ExecuteAsync(postPermission, permissionUri, authorizationValue);
        }

        public async Task<AddPermissionResponse> AddPermissionByResolvingUrlAsync(
            PostPermission postPermission,
            string configurationUrl,
            string authorizationValue)
        {
            if (string.IsNullOrWhiteSpace(configurationUrl))
            {
                throw new ArgumentNullException(nameof(configurationUrl));
            }
            
            Uri uri = null;
            if (!Uri.TryCreate(configurationUrl, UriKind.Absolute, out uri))
            {
                throw new ArgumentException(string.Format(ErrorDescriptions.TheUriIsNotWellFormed, configurationUrl));
            }

            return await AddPermissionByResolvingUrlAsync(postPermission,
                uri,
                authorizationValue);
        }

        public async Task<AddPermissionResponse> AddPermissionByResolvingUrlAsync(
            PostPermission postPermission,
            Uri configurationUri,
            string authorizationValue)
        {
            if (configurationUri == null)
            {
                throw new ArgumentNullException(nameof(configurationUri));
            }

            var configuration = await _getConfigurationOperation.ExecuteAsync(configurationUri);
            return await AddPermissionAsync(postPermission,
                configuration.PermissionRegistrationEndPoint,
                authorizationValue);
        }

        #endregion
    }
}
