using SimpleIdentityServer.Core.Models;
using System.Collections.Generic;

namespace SimpleIdentityServer.Core.Repositories
{
    public interface ITranslationRepository
    {
        Models.Translation GetTranslationByCode(string languageTag, string code);

        List<Models.Translation> GetTranslations(string languageTag);

        List<string> GetSupportedLanguageTag();
    }
}
