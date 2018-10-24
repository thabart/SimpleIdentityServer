using SimpleIdentityServer.Core.Common;
using SimpleIdentityServer.Core.Common.Extensions;
using SimpleIdentityServer.Core.Common.Models;
using SimpleIdentityServer.Core.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace SimpleIdServer.Openid.Server
{
    public static class DefaultConfiguration
    {
        public static List<JsonWebKey> GetJsonWebKeys()
        {
            var path = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "certificate_prk.pfx");
            var certificate = new X509Certificate2(path, string.Empty, X509KeyStorageFlags.Exportable);
            var serializedRsa = ((RSACryptoServiceProvider)certificate.PrivateKey).ToXmlStringNetCore(true);
            return new List<JsonWebKey>
            {
                new JsonWebKey
                {
                    Alg = AllAlg.RS256,
                    KeyOps = new[]
                        {
                            KeyOperations.Sign,
                            KeyOperations.Verify
                        },
                    Kid = "1",
                    Kty = KeyType.RSA,
                    Use = Use.Sig,
                    SerializedKey = serializedRsa
                }
            };
        }

        public static List<Client> GetClients()
        {
            return new List<Client>
            {
                new Client
                {
                    ClientId = "api",
                    ClientName = "api",
                    Secrets = new List<ClientSecret>
                    {
                        new ClientSecret
                        {
                            Type = ClientSecretTypes.SharedSecret,
                            Value = "api"
                        }
                    },
                    TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.client_secret_basic,
                    LogoUri = "http://img.over-blog-kiwi.com/1/47/73/14/20150513/ob_06dc4f_chiot-shiba-inu-a-vendre-prix-2015.jpg",
                    PolicyUri = "http://openid.net",
                    TosUri = "http://openid.net",
                    AllowedScopes = new List<Scope>
                    {
                        new Scope
                        {
                            Name = "register_client"
                        }
                    },
                    GrantTypes = new List<GrantType>
                    {
                        GrantType.client_credentials
                    },
                    ResponseTypes = new List<ResponseType>
                    {
                         ResponseType.token
                    },
                    ApplicationType = ApplicationTypes.native,
                    UpdateDateTime = DateTime.UtcNow,
                    CreateDateTime = DateTime.UtcNow

                }
            };
        }

        public static List<ResourceOwner> GetUsers()
        {
            return new List<ResourceOwner>
            {
                new ResourceOwner
                {
                    Id = "administrator",
                    Claims = new List<Claim>
                    {
                        new Claim(SimpleIdentityServer.Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject, "administrator"),
                        new Claim(SimpleIdentityServer.Core.Jwt.Constants.StandardResourceOwnerClaimNames.Role, "administrator")
                    },
                    Password = PasswordHelper.ComputeHash("password"),
                    IsLocalAccount = true
                }
            };
        }

        public static List<Translation> GetTranslations()
        {
            return new List<Translation>
            {
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
                    Code = SimpleIdentityServer.Core.Constants.StandardTranslationCodes.NameCode,
                    Value = "Name"
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
                }
            };
        }
    }
}