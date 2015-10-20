using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

using SimpleIdentityServer.DataAccess.Mappings;
using SimpleIdentityServer.DataAccess.SqlServer.Models;

namespace SimpleIdentityServer.DataAccess
{
    public class Model : DbContext
    {
        public Model()
            : base("name=Model")
        {
        }

        public virtual IDbSet<Client> Clients { get; set; }

        // public virtual IDbSet<GrantedToken> GrantedTokens { get; set; }

        // public virtual IDbSet<ResourceOwner> ResourceOwners { get; set; }

        public virtual IDbSet<Scope> Scopes { get; set; }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            modelBuilder.Configurations.Add(new ClientMapping());
        }
    }
}