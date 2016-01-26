using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using SimpleIdentityServer.DataAccess.SqlServer.Mappings;
using SimpleIdentityServer.DataAccess.SqlServer.Models;

namespace SimpleIdentityServer.DataAccess.SqlServer
{
    public class SimpleIdentityServerContext : DbContext
    {
        public SimpleIdentityServerContext()
            : base("name=SimpleIdentityServerContext")
        {
        }

        public virtual IDbSet<Translation> Translations { get; set; }

        public virtual IDbSet<Scope> Scopes { get; set; }

        public virtual IDbSet<Claim> Claims { get; set; }

        public virtual IDbSet<Address> Addresses { get; set; }

        public virtual IDbSet<ResourceOwner> ResourceOwners { get; set; }

        public virtual IDbSet<JsonWebKey> JsonWebKeys { get; set; } 
        
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            modelBuilder.Configurations.Add(new TranslationMapping());
            modelBuilder.Configurations.Add(new ScopeMapping());
            modelBuilder.Configurations.Add(new ClaimMapping());
            modelBuilder.Configurations.Add(new AddressMapping());
            modelBuilder.Configurations.Add(new ResourceOwnerMapping());
            modelBuilder.Configurations.Add(new JsonWebKeyMapping());
        }
    }
}