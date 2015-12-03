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
            throw new NotImplementedException();
        }

        public Translation GetTranslationByCode(string languageTag, string code)
        {
            var translation = FakeDataSource.Instance().Translations.FirstOrDefault(t => t.LanguageTag == languageTag && t.Code == code);
            return translation.ToBusiness();
        }
    }
}
