using System.Collections.Generic;
using System.Linq;

using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.DataAccess.Fake.Extensions;

namespace SimpleIdentityServer.DataAccess.Fake.Repositories
{
    public class FakeClientRepository : IClientRepository
    {
        private readonly FakeDataSource _fakeDataSource;
        
        public FakeClientRepository(FakeDataSource fakeDataSource) 
        {
            _fakeDataSource = fakeDataSource;
        }
        
        public Client GetClientById(string clientId)
        {
            var record = _fakeDataSource.Clients.SingleOrDefault(c => c.ClientId == clientId);
            if (record == null)
            {
                return null;
            }

            return record.ToBusiness();
        }

        public bool InsertClient(Client client)
        {
            var record = client.ToFake();
            _fakeDataSource.Clients.Add(record);
            return true;
        }
        
        public IList<Client> GetAll()
        {
            return _fakeDataSource.Clients.Select(c => c.ToBusiness()).ToList();
        }
        
        public bool DeleteClient(Client client)
        {
            var clientToBeRemoved = _fakeDataSource.Clients.FirstOrDefault(c => c.ClientId == client.ClientId);
            if (clientToBeRemoved == null)
            {
                return false;
            }

            _fakeDataSource.Clients.Remove(clientToBeRemoved);
            return true;
        }
    }
}
