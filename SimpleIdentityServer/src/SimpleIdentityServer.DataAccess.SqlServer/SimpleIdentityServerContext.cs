using SimpleIdentityServer.DataAccess.SqlServer.Mappings;
using SimpleIdentityServer.DataAccess.SqlServer.Models;
using Microsoft.Data.Entity;

namespace SimpleIdentityServer.DataAccess.SqlServer
{
    public class SimpleIdentityServerContext : DbContext
    {
        public virtual DbSet<Translation> Translations { get; set; }

        public virtual DbSet<Scope> Scopes { get; set; }

        public virtual DbSet<Claim> Claims { get; set; }

        public virtual DbSet<Address> Addresses { get; set; }

        public virtual DbSet<ResourceOwner> ResourceOwners { get; set; }

        public virtual DbSet<JsonWebKey> JsonWebKeys { get; set; } 

        public virtual DbSet<GrantedToken> GrantedTokens { get; set; }

        public virtual DbSet<Client> Clients { get; set; } 

        public virtual DbSet<Consent> Consents { get; set; }

        public virtual DbSet<AuthorizationCode> AuthorizationCodes { get; set; } 
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.AddTranslationMapping();
            modelBuilder.AddScopeMapping();
            modelBuilder.AddClaimMapping();
            modelBuilder.AddAddressMapping();
            modelBuilder.AddJsonWebKeyMapping();
            modelBuilder.AddGrantedTokenMapping();
            modelBuilder.AddClientMapping();
            modelBuilder.AddConsentMapping();
            modelBuilder.AddAuthorizationCodeMapping();
            base.OnModelCreating(modelBuilder);
            /*
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            modelBuilder.Configurations.Add(new TranslationMapping());
            modelBuilder.Configurations.Add(new ScopeMapping());
            modelBuilder.Configurations.Add(new ClaimMapping());
            modelBuilder.Configurations.Add(new AddressMapping());
            modelBuilder.Configurations.Add(new ResourceOwnerMapping());
            modelBuilder.Configurations.Add(new JsonWebKeyMapping());
            modelBuilder.Configurations.Add(new GrantedTokenMapping());
            modelBuilder.Configurations.Add(new ClientMapping());
            modelBuilder.Configurations.Add(new ConsentMapping());
            modelBuilder.Configurations.Add(new AuthorizationCodeMapping());
            */
        }
    }
}