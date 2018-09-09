using SimpleIdentityServer.Core.Common.Repositories;
using SimpleIdentityServer.Core.Exceptions;
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Core.Api.Profile.Actions
{
    public interface IUnlinkProfileAction
    {
        Task<bool> Execute(string localSubject, string externalSubject);
    }

    internal sealed class UnlinkProfileAction : IUnlinkProfileAction
    {
        private readonly IResourceOwnerRepository _resourceOwnerRepository;
        private readonly IProfileRepository _profileRepository;
        
        public UnlinkProfileAction(IResourceOwnerRepository resourceOwnerRepository, IProfileRepository profileRepository)
        {
            _resourceOwnerRepository = resourceOwnerRepository;
            _profileRepository = profileRepository;
        }

        public async Task<bool> Execute(string localSubject, string externalSubject)
        {
            if (string.IsNullOrWhiteSpace(localSubject))
            {
                throw new ArgumentNullException(nameof(localSubject));
            }

            if (string.IsNullOrWhiteSpace(externalSubject))
            {
                throw new ArgumentNullException(nameof(externalSubject));
            }
            
            var resourceOwner = await _resourceOwnerRepository.GetAsync(localSubject).ConfigureAwait(false);
            if (resourceOwner == null)
            {
                throw new IdentityServerException(Errors.ErrorCodes.InternalError, Errors.ErrorDescriptions.TheResourceOwnerDoesntExist);
            }
            
            var profile = await _profileRepository.Get(externalSubject).ConfigureAwait(false);
            if (profile == null || profile.ResourceOwnerId != localSubject)
            {
                throw new IdentityServerException(Errors.ErrorCodes.InternalError, Errors.ErrorDescriptions.NotAuthorizedToRemoveTheProfile);
            }

            if (profile.Subject == localSubject)
            {
                throw new IdentityServerException(Errors.ErrorCodes.InternalError, Errors.ErrorDescriptions.TheExternalAccountAccountCannotBeUnlinked);
            }

            return await _profileRepository.Remove(new[] { externalSubject }).ConfigureAwait(false);
        }
    }
}
