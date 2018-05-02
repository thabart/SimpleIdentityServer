using SimpleIdentityServer.Manager.Client.DTOs.Responses;
using SimpleIdentityServer.Manager.Common.Requests;
using SimpleIdentityServer.Manager.Common.Responses;
using SimpleIdentityServer.ResourceManager.Core.Api.Clients.Actions;
using SimpleIdentityServer.ResourceManager.Core.Models;
using System.Threading.Tasks;

namespace SimpleIdentityServer.ResourceManager.Core.Api.Clients
{
    public interface IClientActions
    {
        Task<AddClientResponse> Add(string subject, ClientResponse client, EndpointTypes type);
        Task<BaseResponse> Delete(string subject, string clientId, EndpointTypes type);
        Task<GetClientResponse> Get(string subject, string clientId, EndpointTypes type);
        Task<SearchClientResponse> Search(string subject, SearchClientsRequest request, EndpointTypes type);
        Task<BaseResponse> Update(string subject, UpdateClientRequest request, EndpointTypes type);
    }

    internal sealed class ClientActions : IClientActions
    {
        private readonly IAddClientAction _addClientAction;
        private readonly IDeleteClientAction _deleteClientAction;
        private readonly IGetClientAction _getClientAction;
        private readonly ISearchClientsAction _searchClientsAction;
        private readonly IUpdateClientAction _updateClientAction;

        public ClientActions(IAddClientAction addClientAction, IDeleteClientAction deleteClientAction, 
            IGetClientAction getClientAction, ISearchClientsAction searchClientsAction,
            IUpdateClientAction updateClientAction)
        {
            _addClientAction = addClientAction;
            _deleteClientAction = deleteClientAction;
            _getClientAction = getClientAction;
            _searchClientsAction = searchClientsAction;
            _updateClientAction = updateClientAction;
        }

        public Task<AddClientResponse> Add(string subject, ClientResponse client, EndpointTypes type)
        {
            return _addClientAction.Execute(subject, client, type);
        }

        public Task<BaseResponse> Delete(string subject, string clientId, EndpointTypes type)
        {
            return _deleteClientAction.Execute(subject, clientId, type);
        }

        public Task<GetClientResponse> Get(string subject, string clientId, EndpointTypes type)
        {
            return _getClientAction.Execute(subject, clientId, type);
        }

        public Task<SearchClientResponse> Search(string subject, SearchClientsRequest request, EndpointTypes type)
        {
            return _searchClientsAction.Execute(subject, request, type);
        }

        public Task<BaseResponse> Update(string subject, UpdateClientRequest request, EndpointTypes type)
        {
            return _updateClientAction.Execute(subject, request, type);
        }
    }
}
