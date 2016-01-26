using System.Collections.Generic;
using System.Linq;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.DataAccess.SqlServer.Extensions;

namespace SimpleIdentityServer.DataAccess.SqlServer.Repositories
{
    public sealed class TranslationRepository : ITranslationRepository
    {
        public Translation GetTranslationByCode(
            string languageTag, 
            string code)
        {
            using (var context = new SimpleIdentityServerContext())
            {
                var result = context.Translations.FirstOrDefault(t => t.Code == code && t.LanguageTag == languageTag);
                return result.ToDomain();
            }
        }

        public List<Translation> GetTranslations(string languageTag)
        {
            using (var context = new SimpleIdentityServerContext())
            {
                var result = context.Translations.Where(t => t.LanguageTag == languageTag).ToList();
                return result.Select(r => r.ToDomain()).ToList();
            }
        }

        public List<string> GetSupportedLanguageTag()
        {
            using (var context = new SimpleIdentityServerContext())
            {
                var result = context.Translations.Select(t => t.LanguageTag).Distinct();
                return result.ToList();
            }
        }
    }
}
