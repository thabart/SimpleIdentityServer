using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

using SimpleIdentityServer.Core.DataAccess;
using SimpleIdentityServer.Core.DataAccess.Models;
using SimpleIdentityServer.DataAccess.Mappings;

namespace SimpleIdentityServer.DataAccess
{
    public class Model : DbContext, IDataSource
    {
        public Model()
            : base("name=Model")
        {
        }

        public virtual IDbSet<Client> Clients { get; set; }

        public virtual IDbSet<GrantedToken> GrantedTokens { get; set; }

        public virtual IDbSet<ResourceOwner> ResourceOwners { get; set; }

        public virtual IDbSet<Scope> Scopes { get; set; }

        void IDataSource.SaveChanges()
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            modelBuilder.Configurations.Add(new ClientMapping());
        }
    }
}