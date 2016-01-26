using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Security.Cryptography;
using SimpleIdentityServer.Core.Extensions;
using SimpleIdentityServer.DataAccess.SqlServer.Models;

namespace SimpleIdentityServer.DataAccess.SqlServer.Migrations
{

    internal sealed class Configuration : DbMigrationsConfiguration<SimpleIdentityServerContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(SimpleIdentityServerContext context)
        {
            InsertClaims(context);
            InsertScopes(context);
            InsertTranslations(context);
            InsertResourceOwners(context);
            InsertJsonWebKeys(context);
        }

        private static void InsertClaims(SimpleIdentityServerContext context)
        {
            context.Claims.AddOrUpdate(new [] {
                new Claim { Code = Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject }
            });
        }

        private static void InsertScopes(SimpleIdentityServerContext context)
        {
            context.Scopes.AddOrUpdate(new [] {
                new Models.Scope
                {
                    Name = "openid",
                    IsExposed = true,
                    IsOpenIdScope = true,
                    IsDisplayedInConsent = true,
                    Description = "access to the openid scope",
                    Type = ScopeType.ProtectedApi
                },
                new Models.Scope
                {
                    Name = "profile",
                    IsExposed = true,
                    IsOpenIdScope = true,
                    Description = "Access to the profile",
                    Claims = new List<Claim>
                    {
                        new Claim { Code = Core.Jwt.Constants.StandardResourceOwnerClaimNames.Name },
                        new Claim { Code = Core.Jwt.Constants.StandardResourceOwnerClaimNames.FamilyName },
                        new Claim { Code = Core.Jwt.Constants.StandardResourceOwnerClaimNames.GivenName },
                        new Claim { Code = Core.Jwt.Constants.StandardResourceOwnerClaimNames.MiddleName },
                        new Claim { Code = Core.Jwt.Constants.StandardResourceOwnerClaimNames.NickName },
                        new Claim { Code = Core.Jwt.Constants.StandardResourceOwnerClaimNames.PreferredUserName },
                        new Claim { Code = Core.Jwt.Constants.StandardResourceOwnerClaimNames.Profile },
                        new Claim { Code = Core.Jwt.Constants.StandardResourceOwnerClaimNames.Picture },
                        new Claim { Code = Core.Jwt.Constants.StandardResourceOwnerClaimNames.WebSite },
                        new Claim { Code = Core.Jwt.Constants.StandardResourceOwnerClaimNames.Gender },
                        new Claim { Code = Core.Jwt.Constants.StandardResourceOwnerClaimNames.BirthDate },
                        new Claim { Code = Core.Jwt.Constants.StandardResourceOwnerClaimNames.ZoneInfo },
                        new Claim { Code = Core.Jwt.Constants.StandardResourceOwnerClaimNames.Locale },
                        new Claim { Code = Core.Jwt.Constants.StandardResourceOwnerClaimNames.UpdatedAt }
                    },
                    Type = ScopeType.ResourceOwner,
                    IsDisplayedInConsent = true
                }, 
                new Models.Scope
                {
                    Name = "email",
                    IsExposed = true,
                    IsOpenIdScope = true,
                    IsDisplayedInConsent = true,
                    Description = "Access to the email",
                    Claims = new List<Claim>
                    {
                        new Claim { Code = Core.Jwt.Constants.StandardResourceOwnerClaimNames.Email },
                        new Claim { Code = Core.Jwt.Constants.StandardResourceOwnerClaimNames.EmailVerified }
                    },
                    Type = ScopeType.ResourceOwner
                },
                new Models.Scope
                {
                    Name = "address",
                    IsExposed = true,
                    IsOpenIdScope = true,
                    IsDisplayedInConsent = true,
                    Description = "Access to the address",
                    Claims = new List<Claim>
                    {
                        new Claim { Code = Core.Jwt.Constants.StandardResourceOwnerClaimNames.Address }
                    },
                    Type = ScopeType.ResourceOwner
                },
                new Models.Scope
                {
                    Name = "phone",
                    IsExposed = true,
                    IsOpenIdScope = true,
                    IsDisplayedInConsent = true,
                    Description = "Access to the phone",
                    Claims = new List<Claim>
                    {
                        new Claim { Code = Core.Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumber },
                        new Claim { Code = Core.Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumberVerified }
                    },
                    Type = ScopeType.ResourceOwner
                }
            });
        }

        private static void InsertTranslations(SimpleIdentityServerContext context)
        {
            context.Translations.AddOrUpdate(new [] {
                new Models.Translation
                {
                    LanguageTag = "en",
                    Code = Core.Constants.StandardTranslationCodes.ApplicationWouldLikeToCode,
                    Value = "the client {0} would like to access"
                },
                new Models.Translation
                {
                    LanguageTag = "en",
                    Code = Core.Constants.StandardTranslationCodes.IndividualClaimsCode,
                    Value = "individual claims"
                },
                new Models.Translation
                {
                    LanguageTag = "en",
                    Code = Core.Constants.StandardTranslationCodes.LoginCode,
                    Value = "Login"
                },
                new Models.Translation
                {
                    LanguageTag = "en",
                    Code = Core.Constants.StandardTranslationCodes.PasswordCode,
                    Value = "Password"
                },
                new Models.Translation
                {
                    LanguageTag = "en",
                    Code = Core.Constants.StandardTranslationCodes.UserNameCode,
                    Value = "Username"
                },
                new Models.Translation
                {
                    LanguageTag = "en",
                    Code = Core.Constants.StandardTranslationCodes.ConfirmCode,
                    Value = "Confirm"
                },
                new Models.Translation
                {
                    LanguageTag = "en",
                    Code = Core.Constants.StandardTranslationCodes.CancelCode,
                    Value = "Cancel"
                },
                new Models.Translation
                {
                    LanguageTag = "en",
                    Code = Core.Constants.StandardTranslationCodes.LoginLocalAccount,
                    Value = "Login with your local account"
                },
                new Models.Translation
                {
                    LanguageTag = "en",
                    Code = Core.Constants.StandardTranslationCodes.LoginExternalAccount,
                    Value = "Login with your external account"
                },
                new Models.Translation
                {
                    LanguageTag = "en",
                    Code = Core.Constants.StandardTranslationCodes.LinkToThePolicy,
                    Value = "policy"
                },
                new Models.Translation
                {
                    LanguageTag = "en",
                    Code = Core.Constants.StandardTranslationCodes.Tos,
                    Value = "Terms of Service"
                },
                // Swedish
                new Models.Translation
                {
                    LanguageTag = "se",
                    Code = Core.Constants.StandardTranslationCodes.ApplicationWouldLikeToCode,
                    Value = "tillämpning {0} skulle vilja:"
                },
                new Models.Translation
                {
                    LanguageTag = "se",
                    Code = Core.Constants.StandardTranslationCodes.IndividualClaimsCode,
                    Value = "enskilda anspråk"
                },
                new Models.Translation
                {
                    LanguageTag = "se",
                    Code = Core.Constants.StandardTranslationCodes.LoginCode,
                    Value = "Logga in"
                },
                new Models.Translation
                {
                    LanguageTag = "se",
                    Code = Core.Constants.StandardTranslationCodes.PasswordCode,
                    Value = "Lösenord"
                },
                new Models.Translation
                {
                    LanguageTag = "se",
                    Code = Core.Constants.StandardTranslationCodes.UserNameCode,
                    Value = "Användarnamn"
                },
                new Models.Translation
                {
                    LanguageTag = "se",
                    Code = Core.Constants.StandardTranslationCodes.ConfirmCode,
                    Value = "bekräfta"
                },
                new Models.Translation
                {
                    LanguageTag = "se",
                    Code = Core.Constants.StandardTranslationCodes.CancelCode,
                    Value = "annullera"
                },
                // French                
                new Models.Translation
                {
                    LanguageTag = "fr",
                    Code = Core.Constants.StandardTranslationCodes.ApplicationWouldLikeToCode,
                    Value = "L'application veut accéder à:"
                },
                new Models.Translation
                {
                    LanguageTag = "fr",
                    Code = Core.Constants.StandardTranslationCodes.IndividualClaimsCode,
                    Value = "Les claims"
                },
                new Models.Translation
                {
                    LanguageTag = "fr",
                    Code = Core.Constants.StandardTranslationCodes.LoginCode,
                    Value = "S'authentifier"
                },
                new Models.Translation
                {
                    LanguageTag = "fr",
                    Code = Core.Constants.StandardTranslationCodes.PasswordCode,
                    Value = "Mot de passe"
                },
                new Models.Translation
                {
                    LanguageTag = "fr",
                    Code = Core.Constants.StandardTranslationCodes.UserNameCode,
                    Value = "Nom d'utilisateur"
                },
                new Models.Translation
                {
                    LanguageTag = "fr",
                    Code = Core.Constants.StandardTranslationCodes.ConfirmCode,
                    Value = "confirmer"
                },
                new Models.Translation
                {
                    LanguageTag = "fr",
                    Code = Core.Constants.StandardTranslationCodes.CancelCode,
                    Value = "annuler"
                }
            });
        }

        private static void InsertResourceOwners(SimpleIdentityServerContext context)
        {
            context.ResourceOwners.AddOrUpdate(new []
            {
                new Models.ResourceOwner
                {
                    Id = "administrator@hotmail.be",
                    Name = "administrator",
                    Address = new Address
                    {
                        Country  = "France"
                    },
                    BirthDate = "1989-10-07",
                    Email = "habarthierry@hotmail.fr",
                    EmailVerified = true,
                    FamilyName = "habart",
                    Gender = "M",
                    GivenName = "Habart Thierry",
                    Locale = "fr-FR",
                    MiddleName = "Thierry",
                    NickName = "Titi",
                    PhoneNumber = "00",
                    PhoneNumberVerified = false,
                    Picture = "https://upload.wikimedia.org/wikipedia/commons/thumb/5/58/Shiba_inu_taiki.jpg/220px-Shiba_inu_taiki.jpg",
                    PreferredUserName = "Thierry",
                    Profile = "http://localhost/profile",
                    UpdatedAt = DateTime.Now.ConvertToUnixTimestamp(),
                    WebSite = "https://github.com/thabart",
                    ZoneInfo = "Europe/Paris",
                    Password = "5E884898DA28047151D0E56F8DC6292773603D0D6AABBDD62A11EF721D1542D8"
                }
            });
        }

        private static void InsertJsonWebKeys(SimpleIdentityServerContext context)
        {
            var serializedRsa = string.Empty;
            using (var provider = new RSACryptoServiceProvider())
            {
                serializedRsa = provider.ToXmlString(true);
            }

            context.JsonWebKeys.AddOrUpdate(new []
            {
                new Models.JsonWebKey
                {
                    Alg = AllAlg.RS256,
                    KeyOps = "0,1",
                    Kid = "1",
                    Kty = KeyType.RSA,
                    Use = Use.Sig,
                    SerializedKey = serializedRsa,
                },
                new Models.JsonWebKey
                {
                    Alg = AllAlg.RSA1_5,
                    KeyOps = "2,3",
                    Kid = "2",
                    Kty = KeyType.RSA,
                    Use = Use.Enc,
                    SerializedKey = serializedRsa,
                }
            });
        }
    }
}
