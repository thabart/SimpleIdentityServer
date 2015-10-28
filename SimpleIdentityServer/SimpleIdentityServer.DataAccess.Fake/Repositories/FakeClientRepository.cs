using System.Collections.Generic;
using System.Linq;

using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Repositories;

using MODELS = SimpleIdentityServer.DataAccess.Fake.Models;
using SimpleIdentityServer.DataAccess.Fake.Extensions;

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

            return record.ToBusiness();
        }

        public bool InsertClient(Client client)
        {
            var record = client.ToFake();
            FakeDataSource.Instance().Clients.Add(record);
            return true;
        }
    }
}
