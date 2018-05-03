using SimpleIdentityServer.Core.Common.Repositories;
using SimpleIdentityServer.Manager.Core.Errors;
using SimpleIdentityServer.Manager.Core.Exceptions;
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Manager.Core.Api.Claims.Actions
{
    public interface IDeleteClaimAction
    {
        Task<bool> Execute(string claimCode);
    }

    internal sealed class DeleteClaimAction : IDeleteClaimAction
    {
        private readonly IClaimRepository _claimRepository;

        public DeleteClaimAction(IClaimRepository claimRepository)
        {
            _claimRepository = claimRepository;
        }

        public async Task<bool> Execute(string claimCode)
        {
            if (string.IsNullOrWhiteSpace(claimCode))
            {
                throw new ArgumentNullException(nameof(claimCode));
            }
            
            var claim = await _claimRepository.GetAsync(claimCode);
            if (claim == null)
            {
                throw new IdentityServerManagerException(ErrorCodes.InvalidRequestCode, ErrorDescriptions.ClaimDoesntExist);
            }

            return await _claimRepository.Delete(claimCode);
        }
    }
}
