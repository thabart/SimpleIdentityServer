using Microsoft.EntityFrameworkCore;
using SimpleIdentityServer.ResourceManager.EF.Models;
using System;

namespace SimpleIdentityServer.ResourceManager.EF.Mappings
{
    internal static class ProfileMapping
    {
        public static ModelBuilder AddProfileMapping(this ModelBuilder modelBuilder)
        {
            if (modelBuilder == null)
            {
                throw new ArgumentNullException(nameof(modelBuilder));
            }

            modelBuilder.Entity<Profile>()
                .ToTable("profiles")
                .HasKey(s => s.Subject);
            return modelBuilder;
        }
    }
}
