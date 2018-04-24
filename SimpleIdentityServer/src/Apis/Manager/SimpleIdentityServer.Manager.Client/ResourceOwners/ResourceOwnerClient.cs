using SimpleIdentityServer.Manager.Client.Configuration;
using SimpleIdentityServer.Manager.Client.DTOs.Responses;
using SimpleIdentityServer.Manager.Common.Requests;
using SimpleIdentityServer.Manager.Common.Responses;
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Manager.Client.ResourceOwners
{
    public interface IResourceOwnerClient
    {
        Task<BaseResponse> ResolveAdd(Uri wellKnownConfigurationUri, AddResourceOwnerRequest request, string authorizationHeaderValue = null);
        Task<BaseResponse> ResolveUpdate(Uri wellKnownConfigurationUri, ResourceOwnerResponse resourceOwner, string authorizationHeaderValue = null);
        Task<GetResourceOwnerResponse> ResolveGet(Uri wellKnownConfigurationUri, string resourceOwnerId, string authorizationHeaderValue = null);
        Task<BaseResponse> ResolvedDelete(Uri wellKnownConfigurationUri, string resourceOwnerId, string authorizationHeaderValue = null);
        Task<GetAllResourceOwnersResponse> GetAll(Uri resourceOwnerUri, string authorizationHeaderValue = null);
        Task<GetAllResourceOwnersResponse> ResolveGetAll(Uri wellKnownConfigurationUri, string authorizationHeaderValue = null);
        Task<SearchResourceOwnerResponse> ResolveSearch(Uri wellKnownConfigurationUri, SearchResourceOwnersRequest searchResourceOwnersRequest, string authorizationHeaderValue = null);
    }

    internal sealed class ResourceOwnerClient : IResourceOwnerClient
    {
        private readonly IAddResourceOwnerOperation _addResourceOwnerOperation;
        private readonly IDeleteResourceOwnerOperation _deleteResourceOwnerOperation;
        private readonly IGetAllResourceOwnersOperation _getAllResourceOwnersOperation;
        private readonly IGetResourceOwnerOperation _getResourceOwnerOperation;
        private readonly IUpdateResourceOwnerOperation _updateResourceOwnerOperation;
        private readonly IConfigurationClient _configurationClient;
        private readonly ISearchResourceOwnersOperation _searchResourceOwnersOperation;

        public ResourceOwnerClient(IAddResourceOwnerOperation addResourceOwnerOperation, IDeleteResourceOwnerOperation deleteResourceOwnerOperation,
            IGetAllResourceOwnersOperation getAllResourceOwnersOperation, IGetResourceOwnerOperation getResourceOwnerOperation,
            IUpdateResourceOwnerOperation updateResourceOwnerOperation, IConfigurationClient configurationClient,
            ISearchResourceOwnersOperation searchResourceOwnersOperation)
        {
            _addResourceOwnerOperation = addResourceOwnerOperation;
            _deleteResourceOwnerOperation = deleteResourceOwnerOperation;
            _getAllResourceOwnersOperation = getAllResourceOwnersOperation;
            _getResourceOwnerOperation = getResourceOwnerOperation;
            _updateResourceOwnerOperation = updateResourceOwnerOperation;
            _configurationClient = configurationClient;
            _searchResourceOwnersOperation = searchResourceOwnersOperation;
        }

        public async Task<BaseResponse> ResolveAdd(Uri wellKnownConfigurationUri, AddResourceOwnerRequest request, string authorizationHeaderValue = null)
        {
            var configuration = await _configurationClient.GetConfiguration(wellKnownConfigurationUri);
            return await _addResourceOwnerOperation.ExecuteAsync(new Uri(configuration.ResourceOwnersEndpoint), request, authorizationHeaderValue);
        }

        public async Task<BaseResponse> ResolveUpdate(Uri wellKnownConfigurationUri, ResourceOwnerResponse resourceOwner, string authorizationHeaderValue = null)
        {
            var configuration = await _configurationClient.GetConfiguration(wellKnownConfigurationUri);
            return await _updateResourceOwnerOperation.ExecuteAsync(new Uri(configuration.ResourceOwnersEndpoint), resourceOwner, authorizationHeaderValue);
        }

        public async Task<GetResourceOwnerResponse> ResolveGet(Uri wellKnownConfigurationUri, string resourceOwnerId, string authorizationHeaderValue = null)
        {
            var configuration = await _configurationClient.GetConfiguration(wellKnownConfigurationUri);
            return await _getResourceOwnerOperation.ExecuteAsync(new Uri(configuration.ResourceOwnersEndpoint + "/" + resourceOwnerId), authorizationHeaderValue);
        }

        public async Task<BaseResponse> ResolvedDelete(Uri wellKnownConfigurationUri, string resourceOwnerId, string authorizationHeaderValue = null)
        {
            var configuration = await _configurationClient.GetConfiguration(wellKnownConfigurationUri);
            return await _deleteResourceOwnerOperation.ExecuteAsync(new Uri(configuration.ResourceOwnersEndpoint + "/" + resourceOwnerId), authorizationHeaderValue);
        }

        public Task<GetAllResourceOwnersResponse> GetAll(Uri resourceOwnerUri, string authorizationHeaderValue = null)
        {
            return _getAllResourceOwnersOperation.ExecuteAsync(resourceOwnerUri, authorizationHeaderValue);
        }

        public async Task<GetAllResourceOwnersResponse> ResolveGetAll(Uri wellKnownConfigurationUri, string authorizationHeaderValue = null)
        {
            var configuration = await _configurationClient.GetConfiguration(wellKnownConfigurationUri);
            return await GetAll(new Uri(configuration.ResourceOwnersEndpoint), authorizationHeaderValue);
        }

        public async Task<SearchResourceOwnerResponse> ResolveSearch(Uri wellKnownConfigurationUri, SearchResourceOwnersRequest searchResourceOwnersRequest, string authorizationHeaderValue = null)
        {
            var configuration = await _configurationClient.GetConfiguration(wellKnownConfigurationUri);
            return await _searchResourceOwnersOperation.ExecuteAsync(new Uri(configuration.ResourceOwnersEndpoint + "/.search"), searchResourceOwnersRequest, authorizationHeaderValue);
        }
    }
}
