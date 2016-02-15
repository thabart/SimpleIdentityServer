using System.Collections.Generic;
using System.Linq;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.DataAccess.SqlServer.Extensions;

namespace SimpleIdentityServer.DataAccess.SqlServer.Repositories
{
    public sealed class TranslationRepository : ITranslationRepository
    {         
        private readonly SimpleIdentityServerContext _context;
        
        public TranslationRepository(SimpleIdentityServerContext context) {
            _context = context;
        }
        public Translation GetTranslationByCode(
            string languageTag, 
            string code)
        {
                var result = _context.Translations.FirstOrDefault(t => t.Code == code && t.LanguageTag == languageTag);
                if (result == null)
                {
                    return null;
                }

                return result.ToDomain();
        }

        public List<Translation> GetTranslations(string languageTag)
        {
                var result = _context.Translations.Where(t => t.LanguageTag == languageTag).ToList();
                return result.Select(r => r.ToDomain()).ToList();
        }

        public List<string> GetSupportedLanguageTag()
        {
                var result = _context.Translations.Select(t => t.LanguageTag).Distinct();
                return result.ToList();
        }
    }
}
