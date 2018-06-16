using Microsoft.EntityFrameworkCore;
using SimpleIdentityServer.EF.Models;
using System;

namespace SimpleIdentityServer.EF.Mappings
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
                .HasKey(c => c.Subject);
            return modelBuilder;
        }
    }
}
