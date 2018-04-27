using SimpleIdentityServer.ResourceManager.Core.Api.Resources.Actions;
using SimpleIdentityServer.Uma.Common.DTOs;
using System.Threading.Tasks;

namespace SimpleIdentityServer.ResourceManager.Core.Api.Resources
{
    public interface IResourcesetActions
    {
        Task<ResourceSetResponse> Get(string url, string resourceId);
        Task<SearchResourceSetResponse> Search(string url, SearchResourceSet searchResourceSet);
        Task<SearchAuthPoliciesResponse> GetAuthPolicies(string url, string resourceId);
    }

    internal sealed class ResourcesetActions : IResourcesetActions
    {
        private readonly ISearchResourcesetAction _searchResourcesetAction;
        private readonly IGetAuthPoliciesByResourceAction _getAuthPoliciesByResourceAction;
        private readonly IGetResourceAction _getResourceAction;

        public ResourcesetActions(ISearchResourcesetAction searchResourcesetAction, IGetAuthPoliciesByResourceAction getAuthPoliciesByResourceAction,
            IGetResourceAction getResourceAction)
        {
            _searchResourcesetAction = searchResourcesetAction;
            _getAuthPoliciesByResourceAction = getAuthPoliciesByResourceAction;
            _getResourceAction = getResourceAction;
        }

        public Task<SearchResourceSetResponse> Search(string url, SearchResourceSet searchResourceSet)
        {
            return _searchResourcesetAction.Execute(url, searchResourceSet);
        }

        public Task<SearchAuthPoliciesResponse> GetAuthPolicies(string url, string resourceId)
        {
            return _getAuthPoliciesByResourceAction.Execute(url, resourceId);
        }

        public Task<ResourceSetResponse> Get(string url, string resourceId)
        {
            return _getResourceAction.Execute(url, resourceId);
        }
    }
}
