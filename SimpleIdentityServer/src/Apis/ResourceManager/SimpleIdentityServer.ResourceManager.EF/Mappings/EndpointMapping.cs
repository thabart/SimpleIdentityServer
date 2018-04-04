using Microsoft.EntityFrameworkCore;
using SimpleIdentityServer.ResourceManager.EF.Models;

namespace SimpleIdentityServer.ResourceManager.EF.Mappings
{
    internal static class EndpointMapping
    {
        public static ModelBuilder AddEndpointMapping(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Endpoint>()
                .ToTable("endpoints")
                .HasKey(s => s.Url);
            return modelBuilder;
        }
    }
}
