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

        static void TestResourceOwnerRepository()
        {
            var resourceOwnerRepository = new ResourceOwnerRepository();
            Console.WriteLine("============================================");
            Console.WriteLine("Get resource owner via his credentials");
            var resourceOwner = resourceOwnerRepository.GetResourceOwnerByCredentials("administrator",
                "5E884898DA28047151D0E56F8DC6292773603D0D6AABBDD62A11EF721D1542D8");
            Console.WriteLine(resourceOwner.GivenName);

            Console.WriteLine("============================================");
            Console.WriteLine("Get the resource owner via his subject");
            resourceOwner = resourceOwnerRepository.GetBySubject("administrator@hotmail.be");
            Console.WriteLine(resourceOwner.BirthDate);
        }

        static void Main(string[] args)
        {
            // TestTranslationRepository();
            // TestScopeRepository();
            TestResourceOwnerRepository();
            Console.ReadLine();
        }
    }
}
