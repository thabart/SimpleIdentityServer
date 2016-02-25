using Microsoft.Data.Entity;
using SimpleIdentityServer.DataAccess.SqlServer.Models;

namespace SimpleIdentityServer.DataAccess.SqlServer.Mappings
{
    public static class ClaimMapping
    {
        public static void AddClaimMapping(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Claim>()
                .ToTable("claims")
                .HasKey(p => p.Code);
            /*
            ToTable("claims");
            HasKey(p => p.Code);
            */
        }
    }
}
