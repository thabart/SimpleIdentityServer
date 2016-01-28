using System.Data.Entity.ModelConfiguration;
using SimpleIdentityServer.DataAccess.SqlServer.Models;

namespace SimpleIdentityServer.DataAccess.SqlServer.Mappings
{
    public sealed class ScopeMapping : EntityTypeConfiguration<Scope>
    {
        public ScopeMapping()
        {
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
        }
    }
}
