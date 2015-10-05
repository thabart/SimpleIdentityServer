using System.Linq;

using SimpleIdentityServer.Core.DataAccess.Models;
using System.Collections.Generic;
using System.Data.Entity;

namespace SimpleIdentityServer.Core.DataAccess
{
    public interface IDataSource
    {
        IDbSet<ResourceOwner> ResourceOwners { get; set; }

        IDbSet<GrantedToken> GrantedTokens { get; set; }
        
        IDbSet<Client> Clients { get; set; }

        void SaveChanges();
    }
}
