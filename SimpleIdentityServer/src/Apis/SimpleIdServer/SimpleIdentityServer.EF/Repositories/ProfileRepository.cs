using Microsoft.EntityFrameworkCore;
using SimpleIdentityServer.Core.Common.Models;
using SimpleIdentityServer.Core.Common.Parameters;
using SimpleIdentityServer.Core.Common.Repositories;
using SimpleIdentityServer.EF.Extensions;
using SimpleIdentityServer.EF.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdentityServer.EF.Repositories
{
    internal sealed class ProfileRepository : IProfileRepository
    {
        private readonly SimpleIdentityServerContext _context;
        
        public ProfileRepository(SimpleIdentityServerContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get the profile.
        /// </summary>
        /// <param name="subject"></param>
        /// <returns></returns>
        public async Task<ResourceOwnerProfile> Get(string subject)
        {
            if (string.IsNullOrWhiteSpace(subject))
            {
                throw new ArgumentNullException(nameof(subject));
            }

            var profile = await _context.Profiles.FirstOrDefaultAsync(c => c.Subject == subject).ConfigureAwait(false);
            return profile?.ToDomain();
        }

        /// <summary>
        /// Add the profiles.
        /// </summary>
        /// <param name="profiles"></param>
        /// <returns></returns>
        public async Task<bool> Add(IEnumerable<ResourceOwnerProfile> profiles)
        {
            if (profiles == null)
            {
                throw new ArgumentNullException(nameof(profiles));
            }

            var result = true;
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var ps = profiles.Select(p => p.ToModel());
                    _context.AddRange(ps);
                    await _context.SaveChangesAsync().ConfigureAwait(false);
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    result = false;
                }
            }

            return result;
        }

        /// <summary>
        /// Search the profiles.
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public async Task<IEnumerable<ResourceOwnerProfile>> Search(SearchProfileParameter parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            IQueryable<Profile> profiles = _context.Profiles;
            if (parameter.ResourceOwnerIds != null && parameter.ResourceOwnerIds.Any())
            {
                profiles = profiles.Where(p => parameter.ResourceOwnerIds.Contains(p.ResourceOwnerId));
            }

            if (parameter.Issuers != null && parameter.Issuers.Any())
            {
                profiles = profiles.Where(p => parameter.Issuers.Contains(p.Issuer));
            }

            var result = await profiles.ToListAsync().ConfigureAwait(false);
            return result.Select(p => p.ToDomain());
        }

        /// <summary>
        /// Remove the profiles.
        /// </summary>
        /// <param name="subjects"></param>
        /// <returns></returns>
        public async Task<bool> Remove(IEnumerable<string> subjects)
        {
            if (subjects == null)
            {
                throw new ArgumentNullException(nameof(subjects));
            }

            var result = true;
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var profiles = await _context.Profiles.Where(p => subjects.Contains(p.Subject)).ToListAsync().ConfigureAwait(false);
                    if (profiles == null || !profiles.Any())
                    {
                        result = false;
                    }
                    else
                    {
                        _context.Profiles.RemoveRange(profiles);
                        await _context.SaveChangesAsync().ConfigureAwait(false);
                        transaction.Commit();
                    }
                }
                catch
                {
                    transaction.Rollback();
                    result = false;
                }
            }

            return result;
        }
    }
}
