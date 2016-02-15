using System.Collections.Generic;

using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Repositories;

using System.Linq;
using SimpleIdentityServer.DataAccess.Fake.Extensions;

namespace SimpleIdentityServer.DataAccess.Fake.Repositories
{
    public class FakeConsentRepository : IConsentRepository
    {        
        private readonly FakeDataSource _fakeDataSource;
        
        public FakeConsentRepository(FakeDataSource fakeDataSource) 
        {
            _fakeDataSource = fakeDataSource;
        }
        
        public List<Consent> GetConsentsForGivenUser(string subject)
        {
            var result = _fakeDataSource.Consents
                .Where(c => c.ResourceOwner.Id == subject);
            if (!result.Any())
            {
                return null;
            }

            return result.Select(c => c.ToBusiness()).ToList();
        }

        public Consent InsertConsent(Consent record)
        {
            var newRecord = record.ToFake();
            _fakeDataSource.Consents.Add(newRecord);
            return null;
        }
        
        public bool DeleteConsent(Consent record)
        {
            var consentToBeRemoved = _fakeDataSource.Consents.FirstOrDefault(c => c.Id == record.Id);
            if (consentToBeRemoved == null)
            {
                return false;
            }

            _fakeDataSource.Consents.Remove(consentToBeRemoved);
            return true;
        }
    }
}
