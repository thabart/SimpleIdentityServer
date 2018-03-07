using Microsoft.EntityFrameworkCore;
using SimpleIdentityServer.ResourceManager.EF.Models;

namespace SimpleIdentityServer.ResourceManager.EF.Mappings
{
    internal static class AssetAuthPolicyMapping
    {
        public static ModelBuilder AddAssetAuthPolicyMapping(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AssetAuthPolicy>()
                .ToTable("assetAuthPolicies")
                .HasKey(s => new
                {
                    s.AssetHash,
                    s.AuthPolicyId
                });
            return modelBuilder;
        }
    }
}
