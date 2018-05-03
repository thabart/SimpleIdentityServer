using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Core.Results;
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Manager.Core.Api.Claims.Actions
{
    public interface ISearchClaimsAction
    {
        Task<SearchClaimsResult> Execute(SearchClaimsParameter parameter);
    }

    internal sealed class SearchClaimsAction : ISearchClaimsAction
    {
        private readonly IClaimRepository _claimRepository;

        public SearchClaimsAction(IClaimRepository claimRepository)
        {
            _claimRepository = claimRepository;
        }

        public Task<SearchClaimsResult> Execute(SearchClaimsParameter parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            return _claimRepository.Search(parameter);
        }
    }
}
