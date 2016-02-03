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
        private readonly FakeDataSource _fakeDataSource;
        
        public FakeTranslationRepository(FakeDataSource fakeDataSource) 
        {
            _fakeDataSource = fakeDataSource;
        }
        
        public List<string> GetSupportedLanguageTag()
        {
            var languageTags = _fakeDataSource.Translations
                .Select(t => t.LanguageTag)
                .Distinct();
            return languageTags.ToList();
        }

        public List<Translation> GetTranslations(string languageTag)
        {
            return _fakeDataSource.Translations
                .Where(t => t.LanguageTag == languageTag)
                .Select(t => t.ToBusiness())
                .ToList();
        }

        public Translation GetTranslationByCode(string languageTag, string code)
        {
            var translation = _fakeDataSource.Translations.FirstOrDefault(t => t.LanguageTag == languageTag && t.Code == code);
            return translation.ToBusiness();
        }
    }
}
