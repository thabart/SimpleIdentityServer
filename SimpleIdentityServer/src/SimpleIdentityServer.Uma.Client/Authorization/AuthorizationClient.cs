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
using SimpleIdentityServer.Client.Extensions;
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Client.Authorization
{
    public interface IAuthorizationClient
    {
        Task<AuthorizationResponse> GetAuthorizationAsync(
            PostAuthorization postAuthorization,
            string authorizationUrl,
            string authorizationValue);

        Task<AuthorizationResponse> GetAuthorizationAsync(
            PostAuthorization postAuthorization,
            Uri authorizationUri,
            string authorizationValue);

        Task<AuthorizationResponse> GetAuthorizationByResolvingUrlAsync(
            PostAuthorization postAuthorization,
            string configurationUrl,
            string authorizationValue);

        Task<AuthorizationResponse> GetAuthorizationByResolvingUrlAsync(
            PostAuthorization postAuthorization,
            Uri configurationUri,
            string authorizationValue);
    }

    internal class AuthorizationClient : IAuthorizationClient
    {
        private readonly IGetAuthorizationOperation _getAuthorizationOperation;

        private readonly IGetConfigurationOperation _getConfigurationOperation;

        #region Constructor

        public AuthorizationClient(
            IGetAuthorizationOperation getAuthorizationOperation,
            IGetConfigurationOperation getConfigurationOperation)
        {
            _getAuthorizationOperation = getAuthorizationOperation;
            _getConfigurationOperation = getConfigurationOperation;
        }

        #endregion

        #region Public methods

        public async Task<AuthorizationResponse> GetAuthorizationAsync(PostAuthorization postAuthorization, string authorizationUrl, string authorizationValue)
        {
            return await GetAuthorizationAsync(postAuthorization, UriHelpers.GetUri(authorizationUrl), authorizationValue);
        }

        public async Task<AuthorizationResponse> GetAuthorizationAsync(PostAuthorization postAuthorization, Uri authorizationUri, string authorizationValue)
        {
            return await _getAuthorizationOperation.ExecuteAsync(postAuthorization, authorizationUri, authorizationValue);
        }

        public async Task<AuthorizationResponse> GetAuthorizationByResolvingUrlAsync(PostAuthorization postAuthorization, string configurationUrl, string authorizationValue)
        {
            return await GetAuthorizationByResolvingUrlAsync(postAuthorization,
                UriHelpers.GetUri(configurationUrl),
                authorizationValue);
        }

        public async Task<AuthorizationResponse> GetAuthorizationByResolvingUrlAsync(PostAuthorization postAuthorization, Uri configurationUri, string authorizationValue)
        {
            var configuration = await _getConfigurationOperation.ExecuteAsync(configurationUri);
            return await GetAuthorizationAsync(postAuthorization,
                configuration.RptEndPoint,
                authorizationValue);
        }

        #endregion
    }
}
