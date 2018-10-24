using Microsoft.EntityFrameworkCore;
using SimpleIdentityServer.AccountFilter.Basic.EF.Models;
using System;

namespace SimpleIdentityServer.AccountFilter.Basic.EF.Mappings
{
    internal static class FilterMapping
    {
        public static ModelBuilder AddFilterMapping(this ModelBuilder modelBuilder)
        {
            if (modelBuilder == null)
            {
                throw new ArgumentNullException(nameof(modelBuilder));
            }

            modelBuilder.Entity<Filter>()
                .ToTable("filters")
                .HasKey(c => c.Id);
            modelBuilder.Entity<Filter>()
                .HasMany(c => c.Rules)
                .WithOne(s => s.Filter)
                .HasForeignKey(s => s.FilterId)
                .OnDelete(DeleteBehavior.Cascade);
            return modelBuilder;
        }
    }
}
