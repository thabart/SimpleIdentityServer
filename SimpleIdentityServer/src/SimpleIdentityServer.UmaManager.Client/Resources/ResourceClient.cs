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
using SimpleIdentityServer.UmaManager.Client.DTOs.Responses;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleIdentityServer.UmaManager.Client.Resources
{
    public interface IResourceClient
    {
        Task<List<ResourceResponse>> GetResources(
            string url,
            string accessToken);

        Task<List<ResourceResponse>> GetResources(
            Uri uri,
            string accessToken);

        Task<ResourceResponse> GetResource(
            string resourceHash,
            string url,
            string accessToken);

        Task<ResourceResponse> GetResource(
            string resourceHash,
            Uri uri,
            string accessToken);

        Task<List<ResourceResponse>> SearchResources(
            SearchResourceRequest searchResourceRequest,
            Uri uri,
            string accessToken);

        Task<List<ResourceResponse>> SearchResources(
            SearchResourceRequest searchResourceRequest,
            string url,
            string accessToken);

        Task<bool> AddControllerAction(
            AddControllerActionRequest request,
            Uri uri,
            string accessToken);
        Task<bool> AddControllerAction(
            AddControllerActionRequest request,
            string url,
            string accessToken);
    }

    internal class ResourceClient : IResourceClient
    {
        private readonly IGetResourceOperation _getResourceOperation;

        private readonly IGetResourcesOperation _getResourcesOperation;

        private readonly ISearchResourceOperation _searchResourceOperation;

        private readonly IAddControllerActionOperation _addControllerActionOperation;

        #region Constructor

        public ResourceClient(
            IGetResourceOperation getResourceOperation,
            IGetResourcesOperation getResourcesOperation,
            ISearchResourceOperation searchResourceOperation,
            IAddControllerActionOperation addControllerActionOperation)
        {
            _getResourceOperation = getResourceOperation;
            _getResourcesOperation = getResourcesOperation;
            _searchResourceOperation = searchResourceOperation;
            _addControllerActionOperation = addControllerActionOperation;
        }

        #endregion

        #region Public methods

        public async Task<List<ResourceResponse>> GetResources(
            string url,
            string accessToken)
        {
            return await GetResources(TryGetUri(url), accessToken).ConfigureAwait(false);
        }

        public async Task<List<ResourceResponse>> GetResources(
            Uri uri,
            string accessToken)
        {
            return await _getResourcesOperation.ExecuteAsync(uri, accessToken).ConfigureAwait(false);
        }

        public async Task<ResourceResponse> GetResource(
            string resourceHash,
            string url,
            string accessToken)
        {
            return await GetResource(resourceHash, TryGetUri(url), accessToken).ConfigureAwait(false);
        }

        public async Task<ResourceResponse> GetResource(
            string resourceHash,
            Uri uri,
            string accessToken)
        {
            return await _getResourceOperation.ExecuteAsync(resourceHash, uri, accessToken).ConfigureAwait(false);
        }

        public async Task<List<ResourceResponse>> SearchResources(
            SearchResourceRequest searchResourceRequest,
            Uri uri,
            string accessToken)
        {
            return await _searchResourceOperation.ExecuteAsync(searchResourceRequest, uri, accessToken).ConfigureAwait(false);
        }

        public async Task<List<ResourceResponse>> SearchResources(
            SearchResourceRequest searchResourceRequest,
            string url,
            string accessToken)
        {
            return await _searchResourceOperation.ExecuteAsync(searchResourceRequest, TryGetUri(url), accessToken).ConfigureAwait(false);
        }

        public async Task<bool> AddControllerAction(AddControllerActionRequest request, Uri uri, string accessToken)
        {
            return await _addControllerActionOperation.ExecuteAsync(request, uri, accessToken).ConfigureAwait(false);
        }

        public async Task<bool> AddControllerAction(AddControllerActionRequest request, string url, string accessToken)
        {
            return await _addControllerActionOperation.ExecuteAsync(request, TryGetUri(url), accessToken).ConfigureAwait(false);
        }

        #endregion

        #region Private static methods

        private static Uri TryGetUri(string url)
        {
            Uri uri = null;
            if (!Uri.TryCreate(url, UriKind.Absolute, out uri))
            {
                throw new ArgumentException($"the url {url} is not well formed");
            }

            return uri;
        }

        #endregion
    }
}
