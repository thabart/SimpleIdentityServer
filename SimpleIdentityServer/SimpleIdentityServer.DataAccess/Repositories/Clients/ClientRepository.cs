using System.Linq;

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
    }
}
