using System.Collections.Generic;
using SimpleIdentityServer.Core.Models;

namespace SimpleIdentityServer.Core.Repositories
{
    public interface IClientRepository
    {
        Client GetClientById(string clientId);

        bool InsertClient(Client client);

        IList<Client> GetAll();
    }
}
