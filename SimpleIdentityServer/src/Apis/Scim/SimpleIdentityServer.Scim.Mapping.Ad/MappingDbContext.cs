using Microsoft.EntityFrameworkCore;
using SimpleIdentityServer.Scim.Mapping.Ad.Mappings;
using SimpleIdentityServer.Scim.Mapping.Ad.Models;
using System;

namespace SimpleIdentityServer.Scim.Mapping.Ad
{
    public class MappingDbContext : DbContext
    {
        public MappingDbContext(DbContextOptions<MappingDbContext> dbContextOptions) : base(dbContextOptions) { }

        public virtual DbSet<AdMapping> Mappings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if(modelBuilder == null)
            {
                throw new ArgumentNullException(nameof(modelBuilder));
            }

            modelBuilder.AddAdMapping();
        }
    }
}
