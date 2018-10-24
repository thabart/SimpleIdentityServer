using SimpleIdentityServer.Core.Common.Models;
using SimpleIdentityServer.Core.Common.Parameters;
using SimpleIdentityServer.Core.Common.Repositories;
using SimpleIdentityServer.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Core.Repositories
{
    internal sealed class DefaultProfileRepository : IProfileRepository
    {
        public List<ResourceOwnerProfile> _profiles;

        public DefaultProfileRepository(List<ResourceOwnerProfile> profiles)
        {
            _profiles = profiles == null ? new List<ResourceOwnerProfile>() : profiles;
        }

        public Task<bool> Add(IEnumerable<ResourceOwnerProfile> profiles)
        {
            if (profiles == null)
            {
                throw new ArgumentNullException(nameof(profiles));
            }

            foreach(var profile in profiles)
            {
                profile.CreateDateTime = DateTime.UtcNow;
            }

            _profiles.AddRange(profiles.Select(p => p.Copy()));
            return Task.FromResult(true);
        }

        public Task<ResourceOwnerProfile> Get(string subject)
        {
            if (string.IsNullOrWhiteSpace(subject))
            {
                throw new ArgumentNullException(nameof(subject));
            }

            var profile = _profiles.FirstOrDefault(p => p.Subject == subject);
            if (profile == null)
            {
                return Task.FromResult((ResourceOwnerProfile)null);
            }

            return Task.FromResult(profile.Copy());
        }

        public Task<bool> Remove(IEnumerable<string> subjects)
        {
            if (subjects == null)
            {
                throw new ArgumentNullException(nameof(subjects));
            }

            var lstIndexToBeRemoved = _profiles.Where(p => subjects.Contains(p.Subject)).Select(p => _profiles.IndexOf(p)).OrderByDescending(p => p);
            foreach(var index in lstIndexToBeRemoved)
            {
                _profiles.RemoveAt(index);
            }

            return Task.FromResult(true);
        }

        public Task<IEnumerable<ResourceOwnerProfile>> Search(SearchProfileParameter parameter)
        {
            if(parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }


            IEnumerable<ResourceOwnerProfile> result = _profiles;
            if (parameter.ResourceOwnerIds != null && parameter.ResourceOwnerIds.Any())
            {
                result = result.Where(p => parameter.ResourceOwnerIds.Contains(p.ResourceOwnerId));
            }

            if (parameter.Issuers != null && parameter.Issuers.Any())
            {
                result = result.Where(p => parameter.Issuers.Contains(p.Issuer));
            }

            return Task.FromResult(result.Select(r => r.Copy()));
        }
    }
}
