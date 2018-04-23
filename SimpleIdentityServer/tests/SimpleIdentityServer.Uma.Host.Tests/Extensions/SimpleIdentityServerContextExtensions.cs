using SimpleIdentityServer.EF;
using SimpleIdentityServer.EF.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SimpleIdentityServer.Uma.Host.Tests.Extensions
{
    public static class SimpleIdentityServerContextExtensions
    {
        public static void EnsureSeedData(this SimpleIdentityServerContext context, SharedContext sharedContext)
        {
            InsertScopes(context);
            InsertTranslations(context);
            InsertJsonWebKeys(context, sharedContext);
            InsertClients(context);
            try
            {
                context.SaveChanges();
            }
            catch { }
        }

        private static void InsertScopes(SimpleIdentityServerContext context)
        {
            if (!context.Scopes.Any())
            {
                context.Scopes.AddRange(new[] {
                        new Scope
                        {
                            Name = "uma_protection",
                            Description = "Access to UMA permission, resource set & token introspection endpoints",
                            IsOpenIdScope = false,
                            IsDisplayedInConsent = true,
                            Type = ScopeType.ProtectedApi
                        },
                        new Scope
                        {
                            Name = "uma_authorization",
                            Description = "Access to the UMA authorization endpoint",
                            IsOpenIdScope = false,
                            IsDisplayedInConsent = true,
                            Type = ScopeType.ProtectedApi
                        }
                    });
            }
        }

        private static void InsertTranslations(SimpleIdentityServerContext context)
        {
            if (!context.Translations.Any())
            {
                context.Translations.AddRange(new[] {
                    new Translation
                    {
                        LanguageTag = "en",
                        Code = SimpleIdentityServer.Core.Constants.StandardTranslationCodes.ApplicationWouldLikeToCode,
                        Value = "the client {0} would like to access"
                    },
                    new Translation
                    {
                        LanguageTag = "en",
                        Code = SimpleIdentityServer.Core.Constants.StandardTranslationCodes.IndividualClaimsCode,
                        Value = "individual claims"
                    },
                    new Translation
                    {
                        LanguageTag = "en",
                        Code = SimpleIdentityServer.Core.Constants.StandardTranslationCodes.LoginCode,
                        Value = "Login"
                    },
                    new Translation
                    {
                        LanguageTag = "en",
                        Code = SimpleIdentityServer.Core.Constants.StandardTranslationCodes.PasswordCode,
                        Value = "Password"
                    },
                    new Translation
                    {
                        LanguageTag = "en",
                        Code = SimpleIdentityServer.Core.Constants.StandardTranslationCodes.UserNameCode,
                        Value = "Username"
                    },
                    new Translation
                    {
                        LanguageTag = "en",
                        Code = SimpleIdentityServer.Core.Constants.StandardTranslationCodes.ConfirmCode,
                        Value = "Confirm"
                    },
                    new Translation
                    {
                        LanguageTag = "en",
                        Code = SimpleIdentityServer.Core.Constants.StandardTranslationCodes.CancelCode,
                        Value = "Cancel"
                    },
                    new Translation
                    {
                        LanguageTag = "en",
                        Code = SimpleIdentityServer.Core.Constants.StandardTranslationCodes.LoginLocalAccount,
                        Value = "Login with your local account"
                    },
                    new Translation
                    {
                        LanguageTag = "en",
                        Code = SimpleIdentityServer.Core.Constants.StandardTranslationCodes.LoginExternalAccount,
                        Value = "Login with your external account"
                    },
                    new Translation
                    {
                        LanguageTag = "en",
                        Code = SimpleIdentityServer.Core.Constants.StandardTranslationCodes.LinkToThePolicy,
                        Value = "policy"
                    },
                    new Translation
                    {
                        LanguageTag = "en",
                        Code = SimpleIdentityServer.Core.Constants.StandardTranslationCodes.Tos,
                        Value = "Terms of Service"
                    },
                    new Translation
                    {
                        LanguageTag = "en",
                        Code = SimpleIdentityServer.Core.Constants.StandardTranslationCodes.SendCode,
                        Value = "Send code"
                    },
                    new Translation
                    {
                        LanguageTag = "en",
                        Code = SimpleIdentityServer.Core.Constants.StandardTranslationCodes.Code,
                        Value = "Code"
                    },
                    new Translation
                    {
                        LanguageTag = "en",
                        Code = SimpleIdentityServer.Core.Constants.StandardTranslationCodes.EditResourceOwner,
                        Value = "Edit resource owner"
                    },
                    new Translation
                    {
                        LanguageTag = "en",
                        Code = SimpleIdentityServer.Core.Constants.StandardTranslationCodes.YourName,
                        Value = "Your name"
                    },
                    new Translation
                    {
                        LanguageTag = "en",
                        Code = SimpleIdentityServer.Core.Constants.StandardTranslationCodes.YourPassword,
                        Value = "Your password"
                    },
                    new Translation
                    {
                        LanguageTag = "en",
                        Code = SimpleIdentityServer.Core.Constants.StandardTranslationCodes.Email,
                        Value = "Email"
                    },
                    new Translation
                    {
                        LanguageTag = "en",
                        Code = SimpleIdentityServer.Core.Constants.StandardTranslationCodes.YourEmail,
                        Value = "Your email"
                    },
                    new Translation
                    {
                        LanguageTag = "en",
                        Code = SimpleIdentityServer.Core.Constants.StandardTranslationCodes.TwoAuthenticationFactor,
                        Value = "Two authentication factor"
                    },
                    new Translation
                    {
                        LanguageTag = "en",
                        Code = SimpleIdentityServer.Core.Constants.StandardTranslationCodes.UserIsUpdated,
                        Value = "User has been updated"
                    },
                    new Translation
                    {
                        LanguageTag = "en",
                        Code = SimpleIdentityServer.Core.Constants.StandardTranslationCodes.SendConfirmationCode,
                        Value = "Send a confirmation code"
                    },
                    new Translation
                    {
                        LanguageTag = "en",
                        Code = SimpleIdentityServer.Core.Constants.StandardTranslationCodes.Phone,
                        Value = "Phone"
                    },
                    new Translation
                    {
                        LanguageTag = "en",
                        Code = SimpleIdentityServer.Core.Constants.StandardTranslationCodes.HashedPassword,
                        Value = "Hashed password"
                    },
                    // Swedish
                    new Translation
                    {
                        LanguageTag = "se",
                        Code = SimpleIdentityServer.Core.Constants.StandardTranslationCodes.ApplicationWouldLikeToCode,
                        Value = "tillämpning {0} skulle vilja:"
                    },
                    new Translation
                    {
                        LanguageTag = "se",
                        Code = SimpleIdentityServer.Core.Constants.StandardTranslationCodes.IndividualClaimsCode,
                        Value = "enskilda anspråk"
                    },
                    new Translation
                    {
                        LanguageTag = "se",
                        Code = SimpleIdentityServer.Core.Constants.StandardTranslationCodes.LoginCode,
                        Value = "Logga in"
                    },
                    new Translation
                    {
                        LanguageTag = "se",
                        Code = SimpleIdentityServer.Core.Constants.StandardTranslationCodes.PasswordCode,
                        Value = "Lösenord"
                    },
                    new Translation
                    {
                        LanguageTag = "se",
                        Code = SimpleIdentityServer.Core.Constants.StandardTranslationCodes.UserNameCode,
                        Value = "Användarnamn"
                    },
                    new Translation
                    {
                        LanguageTag = "se",
                        Code = SimpleIdentityServer.Core.Constants.StandardTranslationCodes.ConfirmCode,
                        Value = "bekräfta"
                    },
                    new Translation
                    {
                        LanguageTag = "se",
                        Code = SimpleIdentityServer.Core.Constants.StandardTranslationCodes.CancelCode,
                        Value = "annullera"
                    },
                    // French                
                    new Translation
                    {
                        LanguageTag = "fr",
                        Code = SimpleIdentityServer.Core.Constants.StandardTranslationCodes.ApplicationWouldLikeToCode,
                        Value = "L'application veut accéder à:"
                    },
                    new Translation
                    {
                        LanguageTag = "fr",
                        Code = SimpleIdentityServer.Core.Constants.StandardTranslationCodes.IndividualClaimsCode,
                        Value = "Les claims"
                    },
                    new Translation
                    {
                        LanguageTag = "fr",
                        Code = SimpleIdentityServer.Core.Constants.StandardTranslationCodes.LoginCode,
                        Value = "S'authentifier"
                    },
                    new Translation
                    {
                        LanguageTag = "fr",
                        Code = SimpleIdentityServer.Core.Constants.StandardTranslationCodes.PasswordCode,
                        Value = "Mot de passe"
                    },
                    new Translation
                    {
                        LanguageTag = "fr",
                        Code = SimpleIdentityServer.Core.Constants.StandardTranslationCodes.UserNameCode,
                        Value = "Nom d'utilisateur"
                    },
                    new Translation
                    {
                        LanguageTag = "fr",
                        Code = SimpleIdentityServer.Core.Constants.StandardTranslationCodes.ConfirmCode,
                        Value = "confirmer"
                    },
                    new Translation
                    {
                        LanguageTag = "fr",
                        Code = SimpleIdentityServer.Core.Constants.StandardTranslationCodes.CancelCode,
                        Value = "annuler"
                    }
                });
            }
        }

        private static void InsertJsonWebKeys(SimpleIdentityServerContext context, SharedContext sharedContext)
        {
            if (!context.JsonWebKeys.Any())
            {
                var serializedRsa = string.Empty;
#if NET461
                using (var provider = new RSACryptoServiceProvider())
                {
                    serializedRsa = provider.ToXmlString(true);
                }
#else
                using (var rsa = new RSAOpenSsl())
                {
                    serializedRsa = rsa.ToXmlString(true);
                }
#endif

                context.JsonWebKeys.AddRange(new[]
                {
                    sharedContext.ModelEncryptionKey,
                    sharedContext.ModelSignatureKey
                });
            }
        }

        private static void InsertClients(SimpleIdentityServerContext context)
        {
            if (!context.Clients.Any())
            {
                context.Clients.AddRange(new[]
                {
                    // Resource server.
                    new SimpleIdentityServer.EF.Models.Client
                    {
                        ClientId = "resource_server",
                        ClientName = "Resource server",
                        ClientSecrets = new List<ClientSecret>
                        {
                            new ClientSecret
                            {
                                Id = Guid.NewGuid().ToString(),
                                Type = SecretTypes.SharedSecret,
                                Value = "resource_server"
                            }
                        },
                        TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.client_secret_post,
                        LogoUri = "http://img.over-blog-kiwi.com/1/47/73/14/20150513/ob_06dc4f_chiot-shiba-inu-a-vendre-prix-2015.jpg",
                        ClientScopes = new List<ClientScope>
                        {
                            new ClientScope
                            {
                                ScopeName = "uma_protection"
                            },
                            new ClientScope
                            {
                                ScopeName = "uma_authorization"
                            }
                        },
                        GrantTypes = "3,5",
                        ResponseTypes = "1",
                        IdTokenSignedResponseAlg = "RS256",
                        ApplicationType = ApplicationTypes.native
                    },
                    // Anonymous.
                    new SimpleIdentityServer.EF.Models.Client
                    {
                        ClientId = "anonymous",
                        ClientName = "Anonymous",
                        ClientSecrets = new List<ClientSecret>
                        {
                            new ClientSecret
                            {
                                Id = Guid.NewGuid().ToString(),
                                Type = SecretTypes.SharedSecret,
                                Value = "anonymous"
                            }
                        },
                        TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.client_secret_post,
                        LogoUri = "http://img.over-blog-kiwi.com/1/47/73/14/20150513/ob_06dc4f_chiot-shiba-inu-a-vendre-prix-2015.jpg",
                        ClientScopes = new List<ClientScope> {},
                        GrantTypes = "3",
                        ResponseTypes = "1",
                        IdTokenSignedResponseAlg = "RS256",
                        ApplicationType = ApplicationTypes.native
                    }
                });
            }
        }

        private static string ComputeHash(string entry)
        {
            using (var sha256 = SHA256.Create())
            {
                var entryBytes = Encoding.UTF8.GetBytes(entry);
                var hash = sha256.ComputeHash(entryBytes);
                return BitConverter.ToString(hash).Replace("-", string.Empty);
            }
        }
    }
}
