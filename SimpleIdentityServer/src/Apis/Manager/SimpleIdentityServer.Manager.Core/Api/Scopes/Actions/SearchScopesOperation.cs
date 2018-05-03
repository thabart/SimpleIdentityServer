using SimpleIdentityServer.Core.Common.Parameters;
using SimpleIdentityServer.Core.Common.Repositories;
using SimpleIdentityServer.Core.Common.Results;
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Manager.Core.Api.Scopes.Actions
{
    public interface ISearchScopesOperation
    {
        Task<SearchScopeResult> Execute(SearchScopesParameter parameter);
    }

    internal sealed class SearchScopesOperation : ISearchScopesOperation
    {
        private readonly IScopeRepository _scopeRepository;

        public SearchScopesOperation(IScopeRepository scopeRepository)
        {
            _scopeRepository = scopeRepository;
        }

        public Task<SearchScopeResult> Execute(SearchScopesParameter parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            return _scopeRepository.Search(parameter);
        }
    }
}
