using Microsoft.EntityFrameworkCore;
using SimpleIdentityServer.Scim.Mapping.Ad.Models;
using System;

namespace SimpleIdentityServer.Scim.Mapping.Ad.Mappings
{
    internal static class AdConfigurationExtensions
    {
        public static ModelBuilder AddAdConfigurationMapping(this ModelBuilder modelBuilder)
        {
            if(modelBuilder == null)
            {
                throw new ArgumentNullException(nameof(modelBuilder));
            }

            modelBuilder.Entity<AdConfiguration>()
                .ToTable("configurations");
            modelBuilder.Entity<AdConfiguration>()
                .HasKey(c => c.Path);
            return modelBuilder;
        }
    }
}
