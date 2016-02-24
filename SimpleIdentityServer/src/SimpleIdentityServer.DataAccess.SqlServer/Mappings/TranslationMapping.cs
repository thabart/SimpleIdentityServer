using Microsoft.Data.Entity;
using SimpleIdentityServer.DataAccess.SqlServer.Models;

namespace SimpleIdentityServer.DataAccess.SqlServer.Mappings
{
    public static class TranslationMapping
    {
        public static void AddTranslationMapping(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Translation>()
                .ToTable("translations")
                .HasKey(p => new { p.Code, p.LanguageTag });
            /*
            ToTable("translations");
            HasKey(p => new {p.Code, p.LanguageTag});
            Property(p => p.Code)
                .HasMaxLength(255);
            Property(p => p.LanguageTag);
            Property(p => p.Value);
            */
        }
    }
}
