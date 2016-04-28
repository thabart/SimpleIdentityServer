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

namespace SimpleIdentityServer.Client.ResourceSet
{
    public interface IResourceSetClient
    {
        Task<AddResourceSetResponse> AddResourceSetAsync(
            PostResourceSet postResourceSet,
            string resourceSetUrl,
            string authorizationHeaderValue);

        Task<AddResourceSetResponse> AddResourceSetAsync(
            PostResourceSet postResourceSet,
            Uri resourceSetUri,
            string authorizationHeaderValue);

        Task<AddResourceSetResponse> AddResourceSetByResolvingUrlAsync(
            PostResourceSet postResourceSet,
            string configurationUrl,
            string authorizationHeaderValue);

        Task<AddResourceSetResponse> AddResourceSetByResolvingUrlAsync(
            PostResourceSet postResourceSet,
            Uri configurationUri,
            string authorizationHeaderValue);

        Task<bool> DeleteResourceSetAsync(
            string resourceSetId,
            string resourceSetUrl,
            string authorizationHeaderValue);

        Task<bool> DeleteResourceSetByResolvingUrlAsync(
            string resourceSetId,
            string configurationUrl,
            string authorizationHeaderValue);
    }

    internal class ResourceSetClient : IResourceSetClient
    {
        private readonly IAddResourceSetOperation _addResourceSetOperation;

        private readonly IDeleteResourceSetOperation _deleteResourceSetOperation;

        private readonly IGetConfigurationOperation _getConfigurationOperation;

        #region Constructor

        public ResourceSetClient(
            IAddResourceSetOperation addResourceSetOperation,
            IDeleteResourceSetOperation deleteResourceSetOperation,
            IGetConfigurationOperation getConfigurationOperation)
        {
            _addResourceSetOperation = addResourceSetOperation;
            _deleteResourceSetOperation = deleteResourceSetOperation;
            _getConfigurationOperation = getConfigurationOperation;
        }

        #endregion

        #region Public methods

        public async Task<AddResourceSetResponse> AddResourceSetAsync(
            PostResourceSet postResourceSet,
            string resourceSetUrl,
            string authorizationHeaderValue)
        {
            return await AddResourceSetAsync(postResourceSet, UriHelpers.GetUri(resourceSetUrl), authorizationHeaderValue);
        }

        public async Task<AddResourceSetResponse> AddResourceSetAsync(
            PostResourceSet postResourceSet,
            Uri resourceSetUri,
            string authorizationHeaderValue)
        {
            return await _addResourceSetOperation.ExecuteAsync(postResourceSet,
                resourceSetUri,
                authorizationHeaderValue);
        }

        public async Task<AddResourceSetResponse> AddResourceSetByResolvingUrlAsync(
            PostResourceSet postResourceSet,
            string configurationUrl,
            string authorizationHeaderValue)
        {
            return await AddResourceSetByResolvingUrlAsync(postResourceSet, UriHelpers.GetUri(configurationUrl), authorizationHeaderValue);
        }

        public async Task<AddResourceSetResponse> AddResourceSetByResolvingUrlAsync(
            PostResourceSet postResourceSet,
            Uri configurationUri,
            string authorizationHeaderValue)
        {
            var configuration = await _getConfigurationOperation.ExecuteAsync(configurationUri);
            return await AddResourceSetAsync(postResourceSet, configuration.ResourceSetRegistrationEndPoint, authorizationHeaderValue);
        }

        public async Task<bool> DeleteResourceSetAsync(string resourceSetId, string resourceSetUrl, string authorizationHeaderValue)
        {
            return await _deleteResourceSetOperation.ExecuteAsync(resourceSetId, resourceSetUrl, authorizationHeaderValue);
        }

        public async Task<bool> DeleteResourceSetByResolvingUrlAsync(string resourceSetId, string configurationUrl, string authorizationHeaderValue)
        {
            var configuration = await _getConfigurationOperation.ExecuteAsync(UriHelpers.GetUri(configurationUrl));
            return await DeleteResourceSetAsync(resourceSetId, configuration.ResourceSetRegistrationEndPoint, authorizationHeaderValue);
        }

        #endregion
    }
}
