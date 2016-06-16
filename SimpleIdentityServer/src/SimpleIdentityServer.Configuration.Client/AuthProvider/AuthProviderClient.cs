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

using SimpleIdentityServer.Configuration.Client.DTOs.Responses;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Configuration.Client.AuthProvider
{
    public interface IAuthProviderClient
    {
        Task<bool> DisableAuthProvider(string name,
            string authenticationProviderUrl,
            string accessToken);

        Task<bool> EnableAuthProvider(string name,
            string authenticationProviderUrl,
            string accessToken);

        Task<AuthenticationProviderResponse> GetAuthProvider(string name,
            string authenticationProviderUrl,
            string accessToken);

        Task<List<AuthenticationProviderResponse>> GetAuthProviders(
            Uri authenticationProviderUri,
            string accessToken);
    }

    internal class AuthProviderClient : IAuthProviderClient
    {
        private readonly IDisableAuthProviderOperation _disableAuthProviderOperation;

        private readonly IEnableAuthProviderOperation _enableAuthProviderOperation;

        private readonly IGetAuthProviderOperation _getAuthProviderOperation;

        private readonly IGetAuthProvidersOperation _getAuthProvidersOperation;

        #region Constructor

        public AuthProviderClient(
            IDisableAuthProviderOperation disableAuthProviderOperation,
            IEnableAuthProviderOperation enableAuthProviderOperation,
            IGetAuthProviderOperation getAuthProviderOperation,
            IGetAuthProvidersOperation getAuthProvidersOperation)
        {
            _disableAuthProviderOperation = disableAuthProviderOperation;
            _enableAuthProviderOperation = enableAuthProviderOperation;
            _getAuthProviderOperation = getAuthProviderOperation;
            _getAuthProvidersOperation = getAuthProvidersOperation;
        }

        #endregion

        #region Public methods

        public async Task<bool> DisableAuthProvider(string name,
            string authenticationProviderUrl,
            string accessToken)
        {
            return await _disableAuthProviderOperation.ExecuteAsync(name, authenticationProviderUrl, accessToken);
        }

        public async Task<bool> EnableAuthProvider(string name,
            string authenticationProviderUrl,
            string accessToken)
        {
            return await _enableAuthProviderOperation.ExecuteAsync(name, authenticationProviderUrl, accessToken);
        }

        public async Task<AuthenticationProviderResponse> GetAuthProvider(string name,
            string authenticationProviderUrl,
            string accessToken)
        {
            return await _getAuthProviderOperation.ExecuteAsync(name, authenticationProviderUrl, accessToken);
        }

        public async Task<List<AuthenticationProviderResponse>> GetAuthProviders(Uri authenticationProviderUri,
            string accessToken)
        {
            return await _getAuthProvidersOperation.ExecuteAsync(authenticationProviderUri, accessToken);
        }

        #endregion
    }
}
