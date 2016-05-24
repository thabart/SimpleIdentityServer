#region copyright
// Copyright 2015 Habart Thierry
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System.Collections.Generic;
using SimpleIdentityServer.Core.Models;

namespace SimpleIdentityServer.Host.Configuration
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
                new Translation
                {
                    LanguageTag = "en",
                    Code = Core.Constants.StandardTranslationCodes.Tos,
                    Value = "Terms of Service"
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