using System;
using SimpleIdentityServer.DataAccess.SqlServer.Repositories;

namespace SimpleIdentityServer.DataAccess.SqlServer.Client
{
    class Program
    {
        static void TestTranslationRepository()
        {
            var translationRepository = new TranslationRepository();
            const string englishCode = "en";
            var languageTags = translationRepository.GetSupportedLanguageTag();
            Console.WriteLine("============================================");
            Console.WriteLine("Supported language tags : ");
            foreach (var languageTag in languageTags)
            {
                Console.WriteLine(languageTag);
            }

            Console.WriteLine("============================================");
            Console.WriteLine("Get english translations");
            var languages = translationRepository.GetTranslations(englishCode);
            foreach (var translation in languages)
            {
                Console.WriteLine(translation.Code);
            }

            Console.WriteLine("============================================");
            Console.WriteLine("Get unique translation");
            var uniqueTranslation = translationRepository.GetTranslationByCode(englishCode, Core.Constants.StandardTranslationCodes.ApplicationWouldLikeToCode);
            Console.WriteLine(uniqueTranslation.Value);
        }

        static void TestScopeRepository()
        {
            var scopeRepository = new ScopeRepository();
            var scope = scopeRepository.GetScopeByName("profile");
            Console.WriteLine("============================================");
            Console.WriteLine("List of claims for the scope : '{0}'", scope.Name);
            foreach (var claim in scope.Claims)
            {
                Console.WriteLine(claim);
            }

            Console.WriteLine("============================================");
            Console.WriteLine("Get all scopes");
            var scopes = scopeRepository.GetAllScopes();
            foreach (var s in scopes)
            {
                Console.WriteLine(s.Name);
            }
        }

        static void Main(string[] args)
        {
            TestTranslationRepository();
            TestScopeRepository();
            Console.ReadLine();
        }
    }
}
