using SimpleIdentityServer.Core.DataAccess.Models;

using System.Data.Entity;

namespace SimpleIdentityServer.Core.DataAccess
{
    public interface IDataSource
    {
        IDbSet<ResourceOwner> ResourceOwners { get; set; }

        IDbSet<GrantedToken> GrantedTokens { get; set; }
        
        IDbSet<Client> Clients { get; set; }

        IDbSet<Scope> Scopes { get; set; }

        void SaveChanges();
    }
}
