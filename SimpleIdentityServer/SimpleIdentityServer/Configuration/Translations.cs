using SimpleIdentityServer.DataAccess.Fake.Models;
using System.Collections.Generic;

namespace SimpleIdentityServer.Api.Configuration
{
    public static class Translations
    {
        public static List<Translation> Get()
        {
            return new List<Translation>
            {
                // English
                new Translation
                {
                    LanguageTag = "en",
                    Code = Core.Constants.StandardTranslationCodes.ApplicationWouldLikeToCode,
                    Value = "the client {0} would like to access"
                },
                new Translation
                {
                    LanguageTag = "en",
                    Code = Core.Constants.StandardTranslationCodes.IndividualClaimsCode,
                    Value = "individual claims"
                },
                new Translation
                {
                    LanguageTag = "en",
                    Code = Core.Constants.StandardTranslationCodes.LoginCode,
                    Value = "Login"
                },
                new Translation
                {
                    LanguageTag = "en",
                    Code = Core.Constants.StandardTranslationCodes.PasswordCode,
                    Value = "Password"
                },
                new Translation
                {
                    LanguageTag = "en",
                    Code = Core.Constants.StandardTranslationCodes.UserNameCode,
                    Value = "Username"
                },
                new Translation
                {
                    LanguageTag = "en",
                    Code = Core.Constants.StandardTranslationCodes.ConfirmCode,
                    Value = "Confirm"
                },
                new Translation
                {
                    LanguageTag = "en",
                    Code = Core.Constants.StandardTranslationCodes.CancelCode,
                    Value = "Cancel"
                },
                new Translation
                {
                    LanguageTag = "en",
                    Code = Core.Constants.StandardTranslationCodes.LoginLocalAccount,
                    Value = "Login with your local account"
                },
                new Translation
                {
                    LanguageTag = "en",
                    Code = Core.Constants.StandardTranslationCodes.LoginExternalAccount,
                    Value = "Login with your external account"
                },
                new Translation
                {
                    LanguageTag = "en",
                    Code = Core.Constants.StandardTranslationCodes.LinkToThePolicy,
                    Value = "policy"
                },
                // Swedish
                new Translation
                {
                    LanguageTag = "se",
                    Code = Core.Constants.StandardTranslationCodes.ApplicationWouldLikeToCode,
                    Value = "tillämpning {0} skulle vilja:"
                },
                new Translation
                {
                    LanguageTag = "se",
                    Code = Core.Constants.StandardTranslationCodes.IndividualClaimsCode,
                    Value = "enskilda anspråk"
                },
                new Translation
                {
                    LanguageTag = "se",
                    Code = Core.Constants.StandardTranslationCodes.LoginCode,
                    Value = "Logga in"
                },
                new Translation
                {
                    LanguageTag = "se",
                    Code = Core.Constants.StandardTranslationCodes.PasswordCode,
                    Value = "Lösenord"
                },
                new Translation
                {
                    LanguageTag = "se",
                    Code = Core.Constants.StandardTranslationCodes.UserNameCode,
                    Value = "Användarnamn"
                },
                new Translation
                {
                    LanguageTag = "se",
                    Code = Core.Constants.StandardTranslationCodes.ConfirmCode,
                    Value = "bekräfta"
                },
                new Translation
                {
                    LanguageTag = "se",
                    Code = Core.Constants.StandardTranslationCodes.CancelCode,
                    Value = "annullera"
                },
                // French                
                new Translation
                {
                    LanguageTag = "fr",
                    Code = Core.Constants.StandardTranslationCodes.ApplicationWouldLikeToCode,
                    Value = "L'application veut accéder à:"
                },
                new Translation
                {
                    LanguageTag = "fr",
                    Code = Core.Constants.StandardTranslationCodes.IndividualClaimsCode,
                    Value = "Les claims"
                },
                new Translation
                {
                    LanguageTag = "fr",
                    Code = Core.Constants.StandardTranslationCodes.LoginCode,
                    Value = "S'authentifier"
                },
                new Translation
                {
                    LanguageTag = "fr",
                    Code = Core.Constants.StandardTranslationCodes.PasswordCode,
                    Value = "Mot de passe"
                },
                new Translation
                {
                    LanguageTag = "fr",
                    Code = Core.Constants.StandardTranslationCodes.UserNameCode,
                    Value = "Nom d'utilisateur"
                },
                new Translation
                {
                    LanguageTag = "fr",
                    Code = Core.Constants.StandardTranslationCodes.ConfirmCode,
                    Value = "confirmer"
                },
                new Translation
                {
                    LanguageTag = "fr",
                    Code = Core.Constants.StandardTranslationCodes.CancelCode,
                    Value = "annuler"
                }
            };
        }
    }
}