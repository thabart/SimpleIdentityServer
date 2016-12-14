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
using SimpleIdentityServer.Client.Extensions;
using SimpleIdentityServer.Uma.Common.DTOs;
using System;
using System.Collections.Generic;
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
        Task<IEnumerable<string>> GetAll(string url, string token);
        Task<IEnumerable<string>> GetAllByResolvingUrl(string url, string token);
        Task<ResourceSetResponse> Get(string id,  string url, string token);
        Task<ResourceSetResponse> GetByResolvingUrl(string id, string url, string token);
    }

    internal class ResourceSetClient : IResourceSetClient
    {
        private readonly IAddResourceSetOperation _addResourceSetOperation;
        private readonly IDeleteResourceSetOperation _deleteResourceSetOperation;
        private readonly IGetResourcesOperation _getResourcesOperation;
        private readonly IGetResourceOperation _getResourceOperation;
        private readonly IGetConfigurationOperation _getConfigurationOperation;

        public ResourceSetClient(
            IAddResourceSetOperation addResourceSetOperation,
            IDeleteResourceSetOperation deleteResourceSetOperation,
            IGetResourcesOperation getResourcesOperation,
            IGetResourceOperation getResourceOperation,
            IGetConfigurationOperation getConfigurationOperation)
        {
            _addResourceSetOperation = addResourceSetOperation;
            _deleteResourceSetOperation = deleteResourceSetOperation;
            _getResourcesOperation = getResourcesOperation;
            _getResourceOperation = getResourceOperation;
            _getConfigurationOperation = getConfigurationOperation;
        }

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

        public Task<IEnumerable<string>> GetAll(string url, string token)
        {
            return _getResourcesOperation.ExecuteAsync(url, token);
        }

        public async Task<IEnumerable<string>> GetAllByResolvingUrl(string url, string token)
        {
            var configuration = await _getConfigurationOperation.ExecuteAsync(UriHelpers.GetUri(url));
            return await GetAll(configuration.ResourceSetRegistrationEndPoint, token);
        }

        public Task<ResourceSetResponse> Get(string id, string url, string token)
        {
            return _getResourceOperation.ExecuteAsync(id, url, token);
        }

        public async Task<ResourceSetResponse> GetByResolvingUrl(string id, string url, string token)
        {
            var configuration = await _getConfigurationOperation.ExecuteAsync(UriHelpers.GetUri(url));
            return await Get(id, configuration.ResourceSetRegistrationEndPoint, token);
        }
    }
}
