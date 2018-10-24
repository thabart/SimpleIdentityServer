using SimpleIdentityServer.Core.Common.Models;
using SimpleIdentityServer.Core.Common.Repositories;
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Core.Api.Profile.Actions
{
    public interface IGetResourceOwnerClaimsAction
    {
        Task<ResourceOwner> Execute(string externalSubject);
    }

    internal sealed class GetResourceOwnerClaimsAction : IGetResourceOwnerClaimsAction
    {
        private readonly IProfileRepository _profileRepository;
        private readonly IResourceOwnerRepository _resourceOwnerRepository;

        public GetResourceOwnerClaimsAction(IProfileRepository profileRepository, IResourceOwnerRepository resourceOwnerRepository)
        {
            _profileRepository = profileRepository;
            _resourceOwnerRepository = resourceOwnerRepository;
        }

        public async Task<ResourceOwner> Execute(string externalSubject)
        {
            if (string.IsNullOrWhiteSpace(externalSubject))
            {
                throw new ArgumentNullException(nameof(externalSubject));
            }

            var profile = await _profileRepository.Get(externalSubject);
            if (profile == null)
            {
                return null;
            }

            return await _resourceOwnerRepository.GetAsync(profile.ResourceOwnerId);
        }
    }
}
