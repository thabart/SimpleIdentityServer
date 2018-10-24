using SimpleIdentityServer.Core.Common.Models;
using SimpleIdentityServer.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace SimpleIdentityServer.Startup
{
    public static class DefaultConfiguration
    {
        public static List<SimpleIdentityServer.Core.Common.Models.Client> GetClients()
        {
            return new List<SimpleIdentityServer.Core.Common.Models.Client>
            {
                new SimpleIdentityServer.Core.Common.Models.Client
                {
                        ClientId = "website",
                        ClientName = "Website",
                        Secrets = new List<ClientSecret>
                        {
                            new ClientSecret
                            {
                                Type = ClientSecretTypes.SharedSecret,
                                Value = "website"
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
                                Name = "openid"
                            },
                            new Scope
                            {
                                Name = "role"
                            },
                            new Scope
                            {
                                Name = "profile"
                            }
                        },
                        GrantTypes = new List<GrantType>
                        {
                            GrantType.@implicit
                        },
                        ResponseTypes = new List<ResponseType>
                        {
                            ResponseType.code,
                            ResponseType.id_token,
                            ResponseType.token
                        },
                        IdTokenSignedResponseAlg = "RS256",
                        ApplicationType = ApplicationTypes.web,
                        RedirectionUrls = new List<string> { "http://localhost:64950/callback" },
                        PostLogoutRedirectUris = new List<string> { "http://localhost:64950/end_session" },
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
                        new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject, "administrator"),
                        new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.Role, "[ 'administrator', 'role']")
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
                    Code = Core.Constants.StandardTranslationCodes.NameCode,
                    Value = "Name"
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
                new Translation
                {
                    LanguageTag = "en",
                    Code = Core.Constants.StandardTranslationCodes.SendCode,
                    Value = "Send code"
                },
                new Translation
                {
                    LanguageTag = "en",
                    Code = Core.Constants.StandardTranslationCodes.Code,
                    Value = "Code"
                },
                new Translation
                {
                    LanguageTag = "en",
                    Code = Core.Constants.StandardTranslationCodes.EditResourceOwner,
                    Value = "Edit resource owner"
                },
                new Translation
                {
                    LanguageTag = "en",
                    Code = Core.Constants.StandardTranslationCodes.YourName,
                    Value = "Your name"
                },
                new Translation
                {
                    LanguageTag = "en",
                    Code = Core.Constants.StandardTranslationCodes.YourPassword,
                    Value = "Your password"
                },
                new Translation
                {
                    LanguageTag = "en",
                    Code = Core.Constants.StandardTranslationCodes.Email,
                    Value = "Email"
                },
                new Translation
                {
                    LanguageTag = "en",
                    Code = Core.Constants.StandardTranslationCodes.YourEmail,
                    Value = "Your email"
                },
                new Translation
                {
                    LanguageTag = "en",
                    Code = Core.Constants.StandardTranslationCodes.TwoAuthenticationFactor,
                    Value = "Two authentication factor"
                },
                new Translation
                {
                    LanguageTag = "en",
                    Code = Core.Constants.StandardTranslationCodes.UserIsUpdated,
                    Value = "User has been updated"
                },
                new Translation
                {
                    LanguageTag = "en",
                    Code = Core.Constants.StandardTranslationCodes.SendConfirmationCode,
                    Value = "Send a confirmation code"
                },
                new Translation
                {
                    LanguageTag = "en",
                    Code = Core.Constants.StandardTranslationCodes.Phone,
                    Value = "Phone"
                },
                new Translation
                {
                    LanguageTag = "en",
                    Code = Core.Constants.StandardTranslationCodes.HashedPassword,
                    Value = "Hashed password"
                }
            };
        }
    }
}