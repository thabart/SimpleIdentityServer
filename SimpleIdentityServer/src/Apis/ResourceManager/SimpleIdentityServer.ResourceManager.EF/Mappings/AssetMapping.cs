using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using SimpleIdentityServer.ResourceManager.EF.Models;

namespace SimpleIdentityServer.ResourceManager.EF.Mappings
{
    internal static class AssetMapping
    {
        public static ModelBuilder AddAssetMapping(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Asset>()
                .ToTable("assets")
                .HasKey(s => s.Hash);
            modelBuilder.Entity<Asset>()
                .HasMany(s => s.Children)
                .WithOne(s => s.Parent)
                .HasForeignKey(s => s.ResourceParentHash)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Asset>()
                .HasMany(s => s.AuthPolicies)
                .WithOne(s => s.Asset)
                .HasForeignKey(s => s.AssetHash)
                .OnDelete(DeleteBehavior.Cascade);
            return modelBuilder;
        }
    }
}
