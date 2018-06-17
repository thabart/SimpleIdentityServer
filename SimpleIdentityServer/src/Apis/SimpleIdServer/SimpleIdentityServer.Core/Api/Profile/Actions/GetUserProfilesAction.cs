using SimpleIdentityServer.Core.Common.Models;
using SimpleIdentityServer.Core.Common.Parameters;
using SimpleIdentityServer.Core.Common.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Core.Api.Profile.Actions
{
    public interface IGetUserProfilesAction
    {
        Task<IEnumerable<ResourceOwnerProfile>> Execute(string subject);
    }

    internal sealed class GetUserProfilesAction : IGetUserProfilesAction
    {
        private readonly IProfileRepository _profileRepository;

        public GetUserProfilesAction(IProfileRepository profileRepository)
        {
            _profileRepository = profileRepository;
        }

        /// <summary>
        /// Get the profiles linked to the user account.
        /// </summary>
        /// <param name="subject"></param>
        /// <returns></returns>
        public Task<IEnumerable<ResourceOwnerProfile>> Execute(string subject)
        {
            if (string.IsNullOrWhiteSpace(subject))
            {
                throw new ArgumentNullException(nameof(subject));
            }

            return _profileRepository.Search(new SearchProfileParameter
            {
                ResourceOwnerIds = new[] { subject }
            });
        }
    }
}
