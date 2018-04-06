using SimpleIdentityServer.Manager.Client.Configuration;
using SimpleIdentityServer.Manager.Client.DTOs.Parameters;
using SimpleIdentityServer.Manager.Client.DTOs.Responses;
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Manager.Client.Clients
{
    public interface IOpenIdClients
    {
        Task<OpenIdClientResponse> ResolveGet(Uri wellKnownConfigurationUri, string clientId, string authorizationHeaderValue = null);
        Task<BaseResponse> ResolvedDelete(Uri wellKnownConfigurationUri, string clientId, string authorizationHeaderValue = null);
        Task<GetAllClientResponse> GetAll(Uri clientsUri, string authorizationHeaderValue = null);
        Task<GetAllClientResponse> ResolveGetAll(Uri wellKnownConfigurationUri, string authorizationHeaderValue = null);
        Task<SearchClientResponse> ResolveSearch(Uri wellKnownConfigurationUri, SearchClientParameter searchClientParameter, string authorizationHeaderValue = null);
    }

    internal sealed class OpenIdClients : IOpenIdClients
    {
        private readonly IGetAllClientsOperation _getAllClientsOperation;
        private readonly IDeleteClientOperation _deleteClientOperation;
        private readonly IGetClientOperation _getClientOperation;
        private readonly ISearchClientOperation _searchClientOperation;
        private readonly IConfigurationClient _configurationClient;

        public OpenIdClients(IGetAllClientsOperation getAllClientsOperation, IDeleteClientOperation deleteClientOperation,
            IGetClientOperation getClientOperation, ISearchClientOperation searchClientOperation, IConfigurationClient configurationClient)
        {
            _getAllClientsOperation = getAllClientsOperation;
            _deleteClientOperation = deleteClientOperation;
            _getClientOperation = getClientOperation;
            _searchClientOperation = searchClientOperation;
            _configurationClient = configurationClient;
        }

        public async Task<OpenIdClientResponse> ResolveGet(Uri wellKnownConfigurationUri, string clientId, string authorizationHeaderValue = null)
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

        public async Task<SearchClientResponse> ResolveSearch(Uri wellKnownConfigurationUri, SearchClientParameter searchClientParameter, string authorizationHeaderValue = null)
        {
            var configuration = await _configurationClient.GetConfiguration(wellKnownConfigurationUri);
            return await _searchClientOperation.ExecuteAsync(new Uri(configuration.ClientsEndpoint + "/.search"), searchClientParameter, authorizationHeaderValue);
        }
    }
}
