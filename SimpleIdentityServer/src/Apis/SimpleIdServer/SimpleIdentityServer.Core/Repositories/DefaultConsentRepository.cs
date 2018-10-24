using SimpleIdentityServer.Core.Common.Models;
using SimpleIdentityServer.Core.Common.Repositories;
using SimpleIdentityServer.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Core.Repositories
{
    internal sealed class DefaultConsentRepository : IConsentRepository
    {
        public ICollection<Consent> _consents;

        public DefaultConsentRepository(ICollection<Consent> consents)
        {
            _consents = consents == null ? new List<Consent>() : consents;
        }

        public Task<bool> DeleteAsync(Consent record)
        {
            if (record == null)
            {
                throw new ArgumentNullException(nameof(record));
            }

            var consent = _consents.FirstOrDefault(c => c.Id == record.Id);
            if (consent == null)
            {
                return Task.FromResult(false);
            }

            _consents.Remove(consent);
            return Task.FromResult(true);
        }

        public Task<IEnumerable<Consent>> GetConsentsForGivenUserAsync(string subject)
        {
            if (string.IsNullOrWhiteSpace(subject))
            {
                throw new ArgumentNullException(nameof(subject));
            }

            return Task.FromResult(_consents.Where(c => c.ResourceOwner.Id == subject).Select(r => r.Copy()));
        }

        public Task<bool> InsertAsync(Consent record)
        {
            if (record == null)
            {
                throw new ArgumentNullException(nameof(record));
            }

            _consents.Add(record.Copy());
            return Task.FromResult(true);
        }
    }
}
