using Microsoft.EntityFrameworkCore;
using SimpleIdentityServer.ResourceManager.EF.Mappings;
using SimpleIdentityServer.ResourceManager.EF.Models;

namespace SimpleIdentityServer.ResourceManager.EF
{
    public class ResourceManagerDbContext : DbContext
    {
        public ResourceManagerDbContext(DbContextOptions dbContextOptions) : base(dbContextOptions)
        {
            try
            {
                Database.ExecuteSqlCommand("SET TRANSACTION ISOLATION LEVEL READ COMITTED;");
            }
            catch { }
        }

        public virtual DbSet<Asset> Assets { get; set; }
        public virtual DbSet<AssetAuthPolicy> AssetAuthPolicies { get; set; }
        public virtual DbSet<Endpoint> Endpoints { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.AddAssetMapping()
                .AddAssetAuthPolicyMapping()
                .AddEndpointMapping();
            base.OnModelCreating(modelBuilder);
        }
    }
}
