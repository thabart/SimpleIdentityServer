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
        
        public IList<Client> GetAll()
        {
            return FakeDataSource.Instance().Clients.Select(c => c.ToBusiness()).ToList();
        }
        
        public bool DeleteClient(Client client)
        {
            var clientToBeRemoved = FakeDataSource.Instance().Clients.FirstOrDefault(c => c.ClientId == client.ClientId);
            if (clientToBeRemoved == null)
            {
                return false;
            }

            FakeDataSource.Instance().Clients.Remove(clientToBeRemoved);
            return true;
        }
    }
}
