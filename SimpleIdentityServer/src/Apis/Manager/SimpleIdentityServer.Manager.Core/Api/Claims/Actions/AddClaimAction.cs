using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Manager.Core.Exceptions;
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Manager.Core.Api.Claims.Actions
{
    public interface IAddClaimAction
    {
        Task<bool> Execute(AddClaimParameter request);
    }

    public class AddClaimAction : IAddClaimAction
    {
        private readonly IClaimRepository _claimRepository;

        public AddClaimAction(IClaimRepository claimRepository)
        {
            _claimRepository = claimRepository;
        }

        public async Task<bool> Execute(AddClaimParameter request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }
            
            if (string.IsNullOrWhiteSpace(request.Code))
            {
                throw new ArgumentNullException(nameof(request.Code));
            }

            var claim = await _claimRepository.GetAsync(request.Code);
            if (claim != null)
            {
                throw new IdentityServerManagerException(Constants.ErrorCodes.InvalidRequestCode, Constants.ErrorDescriptions.ClaimExists);
            }

            return await _claimRepository.InsertAsync(request);
        }
    }
}
