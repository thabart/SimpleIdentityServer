using SimpleIdentityServer.Manager.Client.Configuration;
using SimpleIdentityServer.Manager.Client.DTOs.Responses;
using SimpleIdentityServer.Manager.Common.Requests;
using SimpleIdentityServer.Manager.Common.Responses;
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Manager.Client.Clients
{
    public interface IOpenIdClients
    {
        Task<AddClientResponse> ResolveAdd(Uri wellKnownConfigurationUri, ClientResponse client, string authorizationHeaderValue = null);
        Task<BaseResponse> ResolveUpdate(Uri wellKnownConfigurationUri, UpdateClientRequest client, string authorizationHeaderValue = null);
        Task<GetClientResponse> ResolveGet(Uri wellKnownConfigurationUri, string clientId, string authorizationHeaderValue = null);
        Task<BaseResponse> ResolvedDelete(Uri wellKnownConfigurationUri, string clientId, string authorizationHeaderValue = null);
        Task<GetAllClientResponse> GetAll(Uri clientsUri, string authorizationHeaderValue = null);
        Task<GetAllClientResponse> ResolveGetAll(Uri wellKnownConfigurationUri, string authorizationHeaderValue = null);
        Task<SearchClientResponse> ResolveSearch(Uri wellKnownConfigurationUri, SearchClientsRequest searchClientParameter, string authorizationHeaderValue = null);
    }

    internal sealed class OpenIdClients : IOpenIdClients
    {
        private readonly IAddClientOperation _addClientOperation;
        private readonly IUpdateClientOperation _updateClientOperation;
        private readonly IGetAllClientsOperation _getAllClientsOperation;
        private readonly IDeleteClientOperation _deleteClientOperation;
        private readonly IGetClientOperation _getClientOperation;
        private readonly ISearchClientOperation _searchClientOperation;
        private readonly IConfigurationClient _configurationClient;

        public OpenIdClients(IAddClientOperation addClientOperation, IUpdateClientOperation updateClientOperation,
            IGetAllClientsOperation getAllClientsOperation, IDeleteClientOperation deleteClientOperation,
            IGetClientOperation getClientOperation, ISearchClientOperation searchClientOperation, IConfigurationClient configurationClient)
        {
            _addClientOperation = addClientOperation;
            _updateClientOperation = updateClientOperation;
            _getAllClientsOperation = getAllClientsOperation;
            _deleteClientOperation = deleteClientOperation;
            _getClientOperation = getClientOperation;
            _searchClientOperation = searchClientOperation;
            _configurationClient = configurationClient;
        }

        public async Task<AddClientResponse> ResolveAdd(Uri wellKnownConfigurationUri, ClientResponse client, string authorizationHeaderValue = null)
        {
            var configuration = await _configurationClient.GetConfiguration(wellKnownConfigurationUri);
            return await _addClientOperation.ExecuteAsync(new Uri(configuration.ClientsEndpoint), client, authorizationHeaderValue);
        }

        public async Task<BaseResponse> ResolveUpdate(Uri wellKnownConfigurationUri, UpdateClientRequest client, string authorizationHeaderValue = null)
        {
            var configuration = await _configurationClient.GetConfiguration(wellKnownConfigurationUri);
            return await _updateClientOperation.ExecuteAsync(new Uri(configuration.ClientsEndpoint), client, authorizationHeaderValue);
        }

        public async Task<GetClientResponse> ResolveGet(Uri wellKnownConfigurationUri, string clientId, string authorizationHeaderValue = null)
        {
            var configuration = await _configurationClient.GetConfiguration(wellKnownConfigurationUri);
            return await _getClientOperation.ExecuteAsync(new Uri(configuration.ClientsEndpoint + "/" + clientId), authorizationHeaderValue);
        }

        public async Task<BaseResponse> ResolvedDelete(Uri wellKnownConfigurationUri, string clientId, string authorizationHeaderValue = null)
        {
            var configuration = await _configurationClient.GetConfiguration(wellKnownConfigurationUri);
            return await _deleteClientOperation.ExecuteAsync(new Uri(configuration.ClientsEndpoint + "/" + clientId), authorizationHeaderValue);
        }

        public Task<GetAllClientResponse> GetAll(Uri clientsUri, string authorizationHeaderValue = null)
        {
            return _getAllClientsOperation.ExecuteAsync(clientsUri, authorizationHeaderValue);
        }

        public async Task<GetAllClientResponse> ResolveGetAll(Uri wellKnownConfigurationUri, string authorizationHeaderValue = null)
        {
            var configuration = await _configurationClient.GetConfiguration(wellKnownConfigurationUri);
            return await GetAll(new Uri(configuration.ClientsEndpoint), authorizationHeaderValue);
        }

        public async Task<SearchClientResponse> ResolveSearch(Uri wellKnownConfigurationUri, SearchClientsRequest searchClientParameter, string authorizationHeaderValue = null)
        {
            var configuration = await _configurationClient.GetConfiguration(wellKnownConfigurationUri);
            return await _searchClientOperation.ExecuteAsync(new Uri(configuration.ClientsEndpoint + "/.search"), searchClientParameter, authorizationHeaderValue);
        }
    }
}
