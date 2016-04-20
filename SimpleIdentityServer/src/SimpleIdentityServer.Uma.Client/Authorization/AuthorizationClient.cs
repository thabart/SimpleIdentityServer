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
            if (string.IsNullOrWhiteSpace(authorizationUrl))
            {
                throw new ArgumentNullException(nameof(authorizationUrl));
            }

            Uri uri = null;
            if (!Uri.TryCreate(authorizationUrl, UriKind.Absolute, out uri))
            {
                throw new ArgumentException(string.Format(ErrorDescriptions.TheUriIsNotWellFormed, authorizationUrl));
            }

            return await GetAuthorizationAsync(postAuthorization, uri, authorizationValue);
        }

        public async Task<AuthorizationResponse> GetAuthorizationAsync(PostAuthorization postAuthorization, Uri authorizationUri, string authorizationValue)
        {
            if (postAuthorization == null)
            {
                throw new ArgumentNullException(nameof(postAuthorization));
            }

            if (authorizationUri == null)
            {
                throw new ArgumentNullException(nameof(authorizationUri));
            }

            if (string.IsNullOrWhiteSpace(authorizationValue))
            {
                throw new ArgumentNullException(nameof(authorizationValue));
            }

            return await _getAuthorizationOperation.ExecuteAsync(postAuthorization, authorizationUri, authorizationValue);
        }

        public async Task<AuthorizationResponse> GetAuthorizationByResolvingUrlAsync(PostAuthorization postAuthorization, string configurationUrl, string authorizationValue)
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

            return await GetAuthorizationByResolvingUrlAsync(postAuthorization,
                uri,
                authorizationValue);
        }

        public async Task<AuthorizationResponse> GetAuthorizationByResolvingUrlAsync(PostAuthorization postAuthorization, Uri configurationUri, string authorizationValue)
        {
            if (configurationUri == null)
            {
                throw new ArgumentNullException(nameof(configurationUri));
            }

            var configuration = await _getConfigurationOperation.ExecuteAsync(configurationUri);
            return await GetAuthorizationAsync(postAuthorization,
                configuration.RptEndPoint,
                authorizationValue);
        }

        #endregion
    }
}
