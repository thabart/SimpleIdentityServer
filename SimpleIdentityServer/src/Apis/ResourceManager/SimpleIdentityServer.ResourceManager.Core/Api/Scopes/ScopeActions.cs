using SimpleIdentityServer.Manager.Client.DTOs.Responses;
using SimpleIdentityServer.Manager.Common.Requests;
using SimpleIdentityServer.Manager.Common.Responses;
using SimpleIdentityServer.ResourceManager.Core.Api.Scopes.Actions;
using SimpleIdentityServer.ResourceManager.Core.Models;
using System.Threading.Tasks;

namespace SimpleIdentityServer.ResourceManager.Core.Api.Scopes
{
    public interface IScopeActions
    {
        Task<BaseResponse> Add(string subject, ScopeResponse request, EndpointTypes type);
        Task<BaseResponse> Delete(string subject, string scopeId, EndpointTypes type);
        Task<GetScopeResponse> Get(string subject, string scopeId, EndpointTypes type);
        Task<SearchScopeResponse> Search(string subject, SearchScopesRequest searchScopesRequest, EndpointTypes type);
        Task<BaseResponse> Update(string subject, ScopeResponse request, EndpointTypes type);
    }

    internal sealed class ScopeActions : IScopeActions
    {
        private readonly IAddScopeAction _addScopeAction;
        private readonly IDeleteScopeAction _deleteScopeAction;
        private readonly IGetScopeAction _getScopeAction;
        private readonly ISearchScopesAction _searchScopesAction;
        private readonly IUpdateScopeAction _updateScopeAction;

        public ScopeActions(IAddScopeAction addScopeAction, IDeleteScopeAction deleteScopeAction, IGetScopeAction getScopeAction, ISearchScopesAction searchScopesAction, IUpdateScopeAction updateScopeAction)
        {
            _addScopeAction = addScopeAction;
            _deleteScopeAction = deleteScopeAction;
            _getScopeAction = getScopeAction;
            _searchScopesAction = searchScopesAction;
            _updateScopeAction = updateScopeAction;
        }

        public Task<BaseResponse> Add(string subject, ScopeResponse request, EndpointTypes type)
        {
            return _addScopeAction.Execute(subject, request, type);
        }

        public Task<BaseResponse> Delete(string subject, string scopeId, EndpointTypes type)
        {
            return _deleteScopeAction.Execute(subject, scopeId, type);
        }

        public Task<GetScopeResponse> Get(string subject, string scopeId, EndpointTypes type)
        {
            return _getScopeAction.Execute(subject, scopeId, type);
        }

        public Task<SearchScopeResponse> Search(string subject, SearchScopesRequest searchScopesRequest, EndpointTypes type)
        {
            return _searchScopesAction.Execute(subject, searchScopesRequest, type);
        }

        public Task<BaseResponse> Update(string subject, ScopeResponse request, EndpointTypes type)
        {
            return _updateScopeAction.Execute(subject, request, type);
        }
    }
}
