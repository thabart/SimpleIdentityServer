using SimpleIdentityServer.Manager.Client.DTOs.Responses;
using SimpleIdentityServer.Manager.Common.Requests;
using SimpleIdentityServer.Manager.Common.Responses;
using SimpleIdentityServer.ResourceManager.Core.Api.ResourceOwners.Actions;
using SimpleIdentityServer.ResourceManager.Core.Models;
using System.Threading.Tasks;

namespace SimpleIdentityServer.ResourceManager.Core.Api.ResourceOwners
{
    public interface IResourceOwnerActions
    {
        Task<BaseResponse> Add(string subject, AddResourceOwnerRequest request);
        Task<BaseResponse> Delete(string subject, string resourceOwnerId);
        Task<GetResourceOwnerResponse> Get(string subject, string resourceOwnerId);
        Task<SearchResourceOwnerResponse> Search(string subject, SearchResourceOwnersRequest request);
        Task<BaseResponse> Update(string subject, ResourceOwnerResponse request);
    }

    internal sealed class ResourceOwnerActions : IResourceOwnerActions
    {
        private readonly IAddResourceOwnerAction _addResourceOwnerAction;
        private readonly IDeleteResourceOwnerAction _deleteResourceOwnerAction;
        private readonly IGetResourceOwnerAction _getResourceOwnerAction;
        private readonly ISearchResourceOwnersAction _searchResourceOwnersAction;
        private readonly IUpdateResourceOwnerAction _updateResourceOwnerAction;

        public ResourceOwnerActions(IAddResourceOwnerAction addResourceOwnerAction, IDeleteResourceOwnerAction deleteResourceOwnerAction,
            IGetResourceOwnerAction getResourceOwnerAction, ISearchResourceOwnersAction searchResourceOwnersAction,
            IUpdateResourceOwnerAction updateResourceOwnerAction)
        {
            _addResourceOwnerAction = addResourceOwnerAction;
            _deleteResourceOwnerAction = deleteResourceOwnerAction;
            _getResourceOwnerAction = getResourceOwnerAction;
            _searchResourceOwnersAction = searchResourceOwnersAction;
            _updateResourceOwnerAction = updateResourceOwnerAction;
        }

        public Task<BaseResponse> Add(string subject, AddResourceOwnerRequest request)
        {
            return _addResourceOwnerAction.Execute(subject, request);
        }

        public Task<BaseResponse> Delete(string subject, string resourceOwnerId)
        {
            return _deleteResourceOwnerAction.Execute(subject, resourceOwnerId);
        }

        public Task<GetResourceOwnerResponse> Get(string subject, string resourceOwnerId)
        {
            return _getResourceOwnerAction.Execute(subject, resourceOwnerId);
        }

        public Task<SearchResourceOwnerResponse> Search(string subject, SearchResourceOwnersRequest request)
        {
            return _searchResourceOwnersAction.Execute(subject, request);
        }

        public Task<BaseResponse> Update(string subject, ResourceOwnerResponse request)
        {
            return _updateResourceOwnerAction.Execute(subject, request);
        }
    }
}
