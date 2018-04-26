using SimpleIdentityServer.ResourceManager.Core.Resources.Actions;
using SimpleIdentityServer.Uma.Common.DTOs;
using System.Threading.Tasks;

namespace SimpleIdentityServer.ResourceManager.Core.Api.Resources
{
    public interface IResourcesetActions
    {
        Task<SearchResourceSetResponse> Search(string url, SearchResourceSet searchResourceSet);
    }

    internal sealed class ResourcesetActions : IResourcesetActions
    {
        private readonly ISearchResourcesetAction _searchResourcesetAction;

        public ResourcesetActions(ISearchResourcesetAction searchResourcesetAction)
        {
            _searchResourcesetAction = searchResourcesetAction;
        }

        public Task<SearchResourceSetResponse> Search(string url, SearchResourceSet searchResourceSet)
        {
            return _searchResourcesetAction.Execute(url, searchResourceSet);
        }
    }
}
