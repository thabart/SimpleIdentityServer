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
using SimpleIdentityServer.Client.Errors;
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Client.ResourceSet
{
    public interface IResourceSetClient
    {
        Task<bool> AddResourceSetAsync(
            PostResourceSet postResourceSet,
            string resourceSetUrl,
            string authorizationHeaderValue);

        Task<bool> AddResourceSetAsync(
            PostResourceSet postResourceSet,
            Uri resourceSetUri,
            string authorizationHeaderValue);

        Task<bool> AddResourceSetnByResolvingUrlAsync(
            PostResourceSet postResourceSet,
            string configurationUrl,
            string authorizationHeaderValue);

        Task<bool> AddResourceSetnByResolvingUrlAsync(
            PostResourceSet postResourceSet,
            Uri configurationUri,
            string authorizationHeaderValue);
    }

    internal class ResourceSetClient : IResourceSetClient
    {
        private readonly IAddResourceSetOperation _addResourceSetOperation;

        private readonly IGetConfigurationOperation _getConfigurationOperation;

        #region Constructor

        public ResourceSetClient(
            IAddResourceSetOperation addResourceSetOperation,
            IGetConfigurationOperation getConfigurationOperation)
        {
            _addResourceSetOperation = addResourceSetOperation;
            _getConfigurationOperation = getConfigurationOperation;
        }

        #endregion

        #region Public methods

        public async Task<bool> AddResourceSetAsync(
            PostResourceSet postResourceSet,
            string resourceSetUrl,
            string authorizationHeaderValue)
        {
            if (string.IsNullOrWhiteSpace(resourceSetUrl))
            {
                throw new ArgumentNullException(nameof(resourceSetUrl));
            }

            Uri uri = null;
            if (!Uri.TryCreate(resourceSetUrl, UriKind.Absolute, out uri))
            {
                throw new ArgumentException(string.Format(ErrorDescriptions.TheUriIsNotWellFormed, resourceSetUrl));
            }

            return await AddResourceSet(postResourceSet, uri, authorizationHeaderValue);
        }

        public async Task<bool> AddResourceSetAsync(
            PostResourceSet postResourceSet,
            Uri resourceSetUri,
            string authorizationHeaderValue)
        {
            return await _addResourceSetOperation.ExecuteAsync(postResourceSet,
                resourceSetUri,
                authorizationHeaderValue);
        }

        public async Task<bool> AddResourceSetnByResolvingUrlAsync(
            PostResourceSet postResourceSet,
            string configurationUrl,
            string authorizationHeaderValue)
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

            return await AddResourceSetnByResolvingUrlAsync(postResourceSet, uri, authorizationHeaderValue);
        }

        public async Task<bool> AddResourceSetnByResolvingUrlAsync(
            PostResourceSet postResourceSet,
            Uri configurationUri,
            string authorizationHeaderValue)
        {
            if (configurationUri == null)
            {
                throw new ArgumentNullException(nameof(configurationUri));
            }
            
            var configuration = await _getConfigurationOperation.ExecuteAsync(configurationUri);
            return await AddResourceSetAsync(postResourceSet, configuration.ResourceSetRegistrationEndPoint, authorizationHeaderValue);
        }

        #endregion
    }
}
