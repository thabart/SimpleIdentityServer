using SimpleIdentityServer.Uma.Core.Models;
using SimpleIdentityServer.Uma.Core.Parameters;
using SimpleIdentityServer.Uma.Core.Repositories;
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Uma.Core.Api.ResourceSetController.Actions
{
    public interface ISearchResourceSetOperation
    {
        Task<SearchResourceSetResult> Execute(SearchResourceSetParameter parameter);
    }

    internal sealed class SearchResourceSetOperation : ISearchResourceSetOperation
    {
        private readonly IResourceSetRepository _resourceSetRepository;

        public SearchResourceSetOperation(IResourceSetRepository resourceSetRepository)
        {
            _resourceSetRepository = resourceSetRepository;
        }

        public Task<SearchResourceSetResult> Execute(SearchResourceSetParameter parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            return _resourceSetRepository.Search(parameter);
        }
    }
}
