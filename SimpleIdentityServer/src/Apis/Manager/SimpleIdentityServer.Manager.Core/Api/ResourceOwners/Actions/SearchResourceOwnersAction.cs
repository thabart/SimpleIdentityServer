using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Core.Results;
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Manager.Core.Api.ResourceOwners.Actions
{
    public interface ISearchResourceOwnersAction
    {
        Task<SearchResourceOwnerResult> Execute(SearchResourceOwnerParameter parameter);
    }

    internal sealed class SearchResourceOwnersAction : ISearchResourceOwnersAction
    {
        private readonly IResourceOwnerRepository _resourceOwnerRepository;

        public SearchResourceOwnersAction(IResourceOwnerRepository resourceOwnerRepository)
        {
            _resourceOwnerRepository = resourceOwnerRepository;
        }

        public Task<SearchResourceOwnerResult> Execute(SearchResourceOwnerParameter parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            return _resourceOwnerRepository.Search(parameter);
        }
    }
}
