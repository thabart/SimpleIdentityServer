using SimpleIdentityServer.Core.Common.Models;
using SimpleIdentityServer.Core.Common.Parameters;
using SimpleIdentityServer.Core.Common.Results;
using SimpleIdentityServer.Manager.Core.Api.Claims.Actions;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Manager.Core.Api.Claims
{
    public interface IClaimActions
    {
        Task<bool> Add(AddClaimParameter request);
        Task<bool> Delete(string claimCode);
        Task<ClaimAggregate> Get(string claimCode);
        Task<SearchClaimsResult> Execute(SearchClaimsParameter parameter);
    }

    internal sealed class ClaimActions : IClaimActions
    {
        private readonly IAddClaimAction _addClaimAction;
        private readonly IDeleteClaimAction _deleteClaimAction;
        private readonly IGetClaimAction _getClaimAction;
        private readonly ISearchClaimsAction _searchClaimsAction;

        public ClaimActions(IAddClaimAction addClaimAction, IDeleteClaimAction deleteClaimAction,
            IGetClaimAction getClaimAction, ISearchClaimsAction searchClaimsAction)
        {
            _addClaimAction = addClaimAction;
            _deleteClaimAction = deleteClaimAction;
            _getClaimAction = getClaimAction;
            _searchClaimsAction = searchClaimsAction;
        }

        public Task<bool> Add(AddClaimParameter request)
        {
            return _addClaimAction.Execute(request);
        }

        public Task<bool> Delete(string claimCode)
        {
            return _deleteClaimAction.Execute(claimCode);
        }

        public Task<ClaimAggregate> Get(string claimCode)
        {
            return _getClaimAction.Execute(claimCode);
        }

        public Task<SearchClaimsResult> Execute(SearchClaimsParameter parameter)
        {
            return _searchClaimsAction.Execute(parameter);
        }
    }
}
