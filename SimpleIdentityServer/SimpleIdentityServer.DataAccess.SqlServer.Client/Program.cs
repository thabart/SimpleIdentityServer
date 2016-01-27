using System;
using System.Collections.Generic;
using System.Linq;
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.DataAccess.SqlServer.Repositories;
using SimpleIdentityServer.Core.Models;
using JsonWebKey = SimpleIdentityServer.DataAccess.SqlServer.Models.JsonWebKey;

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

        static void TestJsonWebKeyRepository()
        {
            var jsonWebKeyRepository = new JsonWebKeyRepository();
            Console.WriteLine("============================================");
            Console.WriteLine("Display all the Json Web Keys");
            var jsonWebKeys = jsonWebKeyRepository.GetAll();
            foreach (var jsonWebKey in jsonWebKeys)
            {
                Console.WriteLine(jsonWebKey.Kid);
            }

            Console.WriteLine("============================================");
            Console.WriteLine("Pick-up one json web key");
            var jsonWebK = jsonWebKeyRepository.GetByAlgorithm(Use.Sig, AllAlg.RS256,
                new[] {KeyOperations.Sign, KeyOperations.Verify})
                .First();
            Console.WriteLine(jsonWebK.SerializedKey);

            Console.WriteLine("============================================");
            Console.WriteLine("Display the json web key by its KID");
            jsonWebK = jsonWebKeyRepository.GetByKid("1");
            Console.WriteLine(jsonWebK.Alg);
        }

        static void TestGrantedTokenRepository()
        {
            const string refreshToken = "refreshtoken";
            const string accessToken = "accesstoken";
            const string scope = "openid";
            const string clientId = "clientId";
            var grantedTokenRepository = new GrantedTokenRepository();
            Console.WriteLine("============================================");
            Console.WriteLine("Insert a new token");
            var grantedToken = new GrantedToken
            {
                Scope = scope,
                ExpiresIn = 1000,
                CreateDateTime = DateTime.UtcNow,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                IdTokenPayLoad = new JwsPayload(),
                UserInfoPayLoad = new JwsPayload(),
                ClientId = clientId
            };
            grantedTokenRepository.Insert(grantedToken);
            
            Console.WriteLine("============================================");
            Console.WriteLine("get granted token with refresh token");
            var token = grantedTokenRepository.GetTokenByRefreshToken(refreshToken);
            Console.WriteLine(token.Scope);
            
            Console.WriteLine("============================================");
            Console.WriteLine("get granted token with access token");
            token = grantedTokenRepository.GetToken(accessToken);
            Console.WriteLine(token.ClientId);

            Console.WriteLine("============================================");
            Console.WriteLine("get granted token");
            token = grantedTokenRepository.GetToken(scope,
                clientId,
                new JwsPayload(),
                new JwsPayload());
            Console.WriteLine(token.AccessToken);


            Console.WriteLine("============================================");
            Console.WriteLine("delete the granted token");
            grantedTokenRepository.Delete(grantedToken);
        }

        static void TestClientRepository()
        {
            var clientRepository = new ClientRepository();
            Console.WriteLine("============================================");
            Console.WriteLine("Insert a new client");
            var client = new Core.Models.Client
            {
                ClientId = "client_id",
                JsonWebKeys = new List<Core.Jwt.JsonWebKey>
                {
                    new Core.Jwt.JsonWebKey
                    {
                        Kid = "10"
                    }
                },
                AllowedScopes = new List<Scope>
                {
                    new Scope
                    {
                        Name = "openid"
                    }
                },
                ResponseTypes = new List<ResponseType> {  ResponseType.code }
            };

            clientRepository.InsertClient(client);

            Console.WriteLine("============================================");
            Console.WriteLine("Delete the client");
            clientRepository.DeleteClient(client);
        }

        static void TestConsentRepository()
        {
            var clientRepository = new ClientRepository();
            var consentRepository = new ConsentRepository();
            Console.WriteLine("============================================");
            Console.WriteLine("Insert a new client");
            var client = new Core.Models.Client
            {
                ClientId = "client_id",
                JsonWebKeys = new List<Core.Jwt.JsonWebKey>
                {
                    new Core.Jwt.JsonWebKey
                    {
                        Kid = "10"
                    }
                },
                AllowedScopes = new List<Scope>
                {
                    new Scope
                    {
                        Name = "openid"
                    }
                },
                ResponseTypes = new List<ResponseType> { ResponseType.code }
            };

            clientRepository.InsertClient(client);

            Console.WriteLine("============================================");
            Console.WriteLine("Insert a new consent");
            var consent = new Core.Models.Consent
            {
                GrantedScopes = new List<Scope>
                {
                    new Scope
                    {
                        Name = "openid"
                    }
                },
                ResourceOwner = new ResourceOwner
                {
                    Id = "administrator@hotmail.be"
                },
                Client = client,
                Claims = new List<string>
                {
                    "email_verified"
                }
            };
            var insertedConsent = consentRepository.InsertConsent(consent);
            
            Console.WriteLine("============================================");
            Console.WriteLine("Get the consent");
            var result = consentRepository.GetConsentsForGivenUser("administrator@hotmail.be");
            Console.WriteLine(result.First().Claims.First());
            
            Console.WriteLine("============================================");
            Console.WriteLine("Delete the consent");
            consentRepository.DeleteConsent(insertedConsent);

            Console.WriteLine("============================================");
            Console.WriteLine("Delete the client");
            clientRepository.DeleteClient(client);
        }

        static void TestAuthorizationCodeRepository()
        {
            const string code = "authorization_code";
            var authorizationCodeRepository = new AuthorizationCodeRepository();
            Console.WriteLine("============================================");
            Console.WriteLine("Insert a new authorization code");
            var authorizationCode = new AuthorizationCode
            {
                ClientId = "clientId",
                Code = code,
                IdTokenPayload = new JwsPayload(),
                Scopes = "scopes",
                CreateDateTime = DateTime.Now
            };

            authorizationCodeRepository.AddAuthorizationCode(authorizationCode);

            Console.WriteLine("============================================");
            Console.WriteLine("Get the authorization code");
            var result = authorizationCodeRepository.GetAuthorizationCode(code);
            Console.WriteLine(result.Scopes);

            Console.WriteLine("============================================");
            Console.WriteLine("Remove the authorization code");
            var success = authorizationCodeRepository.RemoveAuthorizationCode(code);
            Console.WriteLine(success);
        }

        static void Main(string[] args)
        {
            // TestTranslationRepository();
            // TestScopeRepository();
            // TestResourceOwnerRepository();
            // TestJsonWebKeyRepository();
            // TestGrantedTokenRepository();
            // TestClientRepository();
            // TestConsentRepository();
            TestAuthorizationCodeRepository();
            Console.ReadLine();
        }
    }
}
