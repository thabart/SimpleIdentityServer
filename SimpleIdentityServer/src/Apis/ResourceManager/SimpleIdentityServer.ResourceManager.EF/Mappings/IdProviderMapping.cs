using Microsoft.EntityFrameworkCore;
using SimpleIdentityServer.ResourceManager.EF.Models;

namespace SimpleIdentityServer.ResourceManager.EF.Mappings
{
    internal static class IdProviderMapping
    {
        public static ModelBuilder AddIdProviderMapping(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<IdProvider>()
                .ToTable("idProviders")
                .HasKey(s => s.OpenIdWellKnownUrl);
            return modelBuilder;
        }
    }
}
