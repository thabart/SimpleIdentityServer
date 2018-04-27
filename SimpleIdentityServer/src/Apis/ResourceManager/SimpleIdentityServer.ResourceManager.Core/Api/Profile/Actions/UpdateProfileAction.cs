using SimpleIdentityServer.ResourceManager.Core.Models;
using SimpleIdentityServer.ResourceManager.Core.Repositories;
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.ResourceManager.Core.Api.Profile.Actions
{
    public interface IUpdateProfileAction
    {
        Task<bool> Execute(ProfileAggregate profile);
    }

    internal sealed class UpdateProfileAction : IUpdateProfileAction
    {
        private readonly IProfileRepository _profileRepository;

        public UpdateProfileAction(IProfileRepository profileRepository)
        {
            _profileRepository = profileRepository;
        }

        public async Task<bool> Execute(ProfileAggregate profile)
        {
            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile));
            }

            var pr = await _profileRepository.Get(profile.Subject);
            if (pr == null)
            {
                return await _profileRepository.Add(new[] { profile });
            }

            return await _profileRepository.Update(new[] { profile });
        }
    }
}
