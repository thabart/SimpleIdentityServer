using System.Data.Entity.ModelConfiguration;
using SimpleIdentityServer.DataAccess.SqlServer.Models;

namespace SimpleIdentityServer.DataAccess.SqlServer.Mappings
{
    public sealed class TranslationMapping : EntityTypeConfiguration<Translation>
    {
        public TranslationMapping()
        {
            ToTable("translations");
            HasKey(p => new {p.Code, p.LanguageTag});
            Property(p => p.Code)
                .HasMaxLength(255);
            Property(p => p.LanguageTag);
            Property(p => p.Value);
        }
    }
}
