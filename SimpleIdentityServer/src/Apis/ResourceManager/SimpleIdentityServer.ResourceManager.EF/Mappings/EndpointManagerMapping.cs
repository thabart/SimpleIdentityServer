using Microsoft.EntityFrameworkCore;
using SimpleIdentityServer.ResourceManager.EF.Models;

namespace SimpleIdentityServer.ResourceManager.EF.Mappings
{
    internal static class EndpointManagerMapping
    {
        public static ModelBuilder AddEndpointManagerMapping(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EndpointManager>()
                .ToTable("endpointManagers")
                .HasKey(s => new { s.SourceUrl, s.AuthUrl });
            modelBuilder.Entity<EndpointManager>()
                .HasOne(s => s.SourceEndpoint)
                .WithOne(s => s.Manager)
                .HasForeignKey<EndpointManager>(s => s.SourceUrl);
            return modelBuilder;
        }
    }
}
