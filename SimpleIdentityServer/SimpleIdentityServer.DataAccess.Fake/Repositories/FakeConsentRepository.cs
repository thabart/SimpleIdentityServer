using System.Collections.Generic;

using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Repositories;

using System.Linq;
using System;

namespace SimpleIdentityServer.DataAccess.Fake.Repositories
{
    public class FakeConsentRepository : IConsentRepository
    {
        public List<Consent> GetConsentsForGivenUser(string subject)
        {
            var result = FakeDataSource.Instance().Consents
                .Where(c => c.ResourceOwner.Id == subject);
            if (!result.Any())
            {
                return null;
            }
            
            return result.Select(c => new Consent
                 {
                     Id = c.Id,
                     Client = new Client
                     {
                         ClientId = c.Client.ClientId,
                         DisplayName = c.Client.DisplayName
                     },
                     ResourceOwner = new ResourceOwner
                     {
                         Id = subject
                     },
                     GrantedScopes = c.GrantedScopes.Select(s => new Scope
                     {
                         Name = s.Name
                     }).ToList()
                 }).ToList();
        }

        public void InsertConsent(Consent record)
        {
            var newRecord = new Fake.Models.Consent
            {
                Id = record.Id,
                Client = new Fake.Models.Client
                {
                    ClientId = record.Client.ClientId
                },
                ResourceOwner = new Fake.Models.ResourceOwner
                {
                    Id = record.ResourceOwner.Id
                },
                GrantedScopes = record.GrantedScopes.Select(s => new Fake.Models.Scope
                {
                    Name = s.Name
                }).ToList()
            };

            FakeDataSource.Instance().Consents.Add(newRecord);
        }
    }
}
