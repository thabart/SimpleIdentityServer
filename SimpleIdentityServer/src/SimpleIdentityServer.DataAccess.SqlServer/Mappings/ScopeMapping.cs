using SimpleIdentityServer.DataAccess.SqlServer.Models;
using Microsoft.Data.Entity;

namespace SimpleIdentityServer.DataAccess.SqlServer.Mappings
{
    public static class ScopeMapping
    {
        public static void AddScopeMapping(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Scope>()
                .ToTable("scopes")
                .HasKey(p => p.Name);
            /*
            ToTable("scopes");
            HasKey(p => p.Name);
            Property(p => p.Description)
                .HasMaxLength(255);
            Property(p => p.IsOpenIdScope);
            Property(p => p.IsDisplayedInConsent);
            Property(p => p.IsExposed);
            Property(p => p.Type);
            HasMany(p => p.Claims)
                .WithMany(p => p.Scopes)
                .Map(c =>
                {
                    c.MapLeftKey("ScopeName");
                    c.MapRightKey("ClaimCode");
                    c.ToTable("scopeClaims");
                });
           */
        }
    }
}
