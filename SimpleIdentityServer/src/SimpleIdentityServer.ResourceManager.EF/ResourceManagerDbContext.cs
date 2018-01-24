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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.AddAssetMapping();
            base.OnModelCreating(modelBuilder);
        }
    }
}
