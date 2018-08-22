using Microsoft.EntityFrameworkCore;
using SimpleIdentityServer.Scim.Mapping.Ad.Models;
using System;

namespace SimpleIdentityServer.Scim.Mapping.Ad.Mappings
{
    internal static class AdMappingExtensions
    {
        public static ModelBuilder AddAdMapping(this ModelBuilder modelBuilder)
        {
            if (modelBuilder == null)
            {
                throw new ArgumentNullException(nameof(modelBuilder));
            }

            modelBuilder.Entity<AdMapping>()
                .ToTable("mappings");
            modelBuilder.Entity<AdMapping>()
                .HasKey(m => m.AttributeId);
            return modelBuilder;
        }
    }
}
