using System.Collections.Generic;
using System.Linq;

using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Repositories;

using MODELS = SimpleIdentityServer.DataAccess.Fake.Models;

namespace SimpleIdentityServer.DataAccess.Fake.Repositories
{
    public class FakeClientRepository : IClientRepository
    {
        public Client GetClientById(string clientId)
        {
            var record = FakeDataSource.Instance().Clients.SingleOrDefault(c => c.ClientId == clientId);
            if (record == null)
            {
                return null;
            }

            var result = new Client
            {
                ClientId = record.ClientId,
                DisplayName = record.DisplayName,
                AllowedScopes = record.AllowedScopes == null || !record.AllowedScopes.Any() ? null : record.AllowedScopes.Select(r => new Scope
                {
                    Name = r.Name
                }).ToList(),
                RedirectionUrls = record.RedirectionUrls == null || !record.RedirectionUrls.Any() ? 
                    null : 
                    record.RedirectionUrls.Select(r => r.Name).ToList()
            };

            return result;
        }

        public bool InsertClient(Client client)
        {
            var record = new MODELS.Client
            {
                ClientId = client.ClientId,
                AllowedScopes = new List<MODELS.Scope>(),
                RedirectionUrls = new List<MODELS.RedirectionUrl>()
            };

            foreach (var scope in client.AllowedScopes)
            {
                var recordScope = new MODELS.Scope
                {
                    Name = scope.Name
                };

                record.AllowedScopes.Add(recordScope);
            }

            foreach (var redirectionUrl in client.RedirectionUrls)
            {
                var recordRedirectionUrl = new MODELS.RedirectionUrl
                {
                    Name = redirectionUrl
                };

                record.RedirectionUrls.Add(recordRedirectionUrl);
            }

            FakeDataSource.Instance().Clients.Add(record);
            return true;
        }
    }
}
