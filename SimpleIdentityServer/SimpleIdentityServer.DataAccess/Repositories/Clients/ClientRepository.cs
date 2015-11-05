using System.Collections.Generic;

using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Repositories;

namespace SimpleIdentityServer.DataAccess.SqlServer.Repositories.Clients
{
    public class ClientRepository : IClientRepository
    {
        public Client GetClientById(string clientId)
        {
            return null;
        }

        public bool InsertClient(Client client)
        {
            return true;
        }

        public IList<Client> GetAll()
        {
            return null;
        }
    }
}
