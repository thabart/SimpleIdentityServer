using System;
using System.Collections.Generic;

using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Core.Models;
using System.Linq;
using SimpleIdentityServer.DataAccess.Fake.Extensions;

namespace SimpleIdentityServer.DataAccess.Fake.Repositories
{
    public class FakeTranslationRepository : ITranslationRepository
    {
        public List<string> GetSupportedLanguageTag()
        {
            var languageTags = FakeDataSource.Instance().Translations
                .Select(t => t.LanguageTag)
                .Distinct();
            return languageTags.ToList();
        }

        public List<Translation> GetTranslations(string languageTag)
        {
            return FakeDataSource.Instance().Translations
                .Where(t => t.LanguageTag == languageTag)
                .Select(t => t.ToBusiness())
                .ToList();
        }

        public Translation GetTranslationByCode(string languageTag, string code)
        {
            var translation = FakeDataSource.Instance().Translations.FirstOrDefault(t => t.LanguageTag == languageTag && t.Code == code);
            return translation.ToBusiness();
        }
    }
}
