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

using SimpleIdentityServer.Core;
using SimpleIdentityServer.Core.Extensions;
using SimpleIdentityServer.DataAccess.SqlServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace SimpleIdentityServer.DataAccess.SqlServer.Extensions
{
    public static class SimpleIdentityServerContextExtensions
    {
        #region Public static methods

        public static void EnsureSeedData(this SimpleIdentityServerContext context)
        {
            InsertRoles(context);
            InsertClaims(context);
            InsertScopes(context);
            InsertTranslations(context);
            InsertResourceOwners(context);
            InsertJsonWebKeys(context);
            InsertClients(context);
            context.SaveChanges();
        }

        #endregion

        #region Private static methods

        private static void InsertRoles(SimpleIdentityServerContext context)
        {
            if (!context.Roles.Any())
            {
                context.Roles.AddRange(new[]
                {
                    new Role
                    {
                        Name = "administrator",
                        Description = "administrator role"
                    }
                });
            }
        }

        private static void InsertClaims(SimpleIdentityServerContext context)
        {
            if (!context.Claims.Any())
            {
                context.Claims.AddRange(new[] {
                    new Claim { Code = Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject },
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
                    new Claim { Code = Core.Jwt.Constants.StandardResourceOwnerClaimNames.UpdatedAt },
                    new Claim { Code = Core.Jwt.Constants.StandardResourceOwnerClaimNames.Email },
                    new Claim { Code = Core.Jwt.Constants.StandardResourceOwnerClaimNames.EmailVerified },
                    new Claim { Code = Core.Jwt.Constants.StandardResourceOwnerClaimNames.Address },
                    new Claim { Code = Core.Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumber },
                    new Claim { Code = Core.Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumberVerified },
                    new Claim { Code = Core.Jwt.Constants.StandardResourceOwnerClaimNames.Role }
                });
            }
        }

        private static void InsertScopes(SimpleIdentityServerContext context)
        {
            if (!context.Scopes.Any())
            {
                context.Scopes.AddRange(new[] {
                    new Scope
                    {
                        Name = "openid",
                        IsExposed = true,
                        IsOpenIdScope = true,
                        IsDisplayedInConsent = true,
                        Description = "access to the openid scope",
                        Type = ScopeType.ProtectedApi
                    },
                    new Scope
                    {
                        Name = "profile",
                        IsExposed = true,
                        IsOpenIdScope = true,
                        Description = "Access to the profile",
                        ScopeClaims = new List<ScopeClaim>
                        {
                            new ScopeClaim { ClaimCode = Core.Jwt.Constants.StandardResourceOwnerClaimNames.Name },
                            new ScopeClaim { ClaimCode = Core.Jwt.Constants.StandardResourceOwnerClaimNames.FamilyName },
                            new ScopeClaim { ClaimCode = Core.Jwt.Constants.StandardResourceOwnerClaimNames.GivenName },
                            new ScopeClaim { ClaimCode = Core.Jwt.Constants.StandardResourceOwnerClaimNames.MiddleName },
                            new ScopeClaim { ClaimCode = Core.Jwt.Constants.StandardResourceOwnerClaimNames.NickName },
                            new ScopeClaim { ClaimCode = Core.Jwt.Constants.StandardResourceOwnerClaimNames.PreferredUserName },
                            new ScopeClaim { ClaimCode = Core.Jwt.Constants.StandardResourceOwnerClaimNames.Profile },
                            new ScopeClaim { ClaimCode = Core.Jwt.Constants.StandardResourceOwnerClaimNames.Picture },
                            new ScopeClaim { ClaimCode = Core.Jwt.Constants.StandardResourceOwnerClaimNames.WebSite },
                            new ScopeClaim { ClaimCode = Core.Jwt.Constants.StandardResourceOwnerClaimNames.Gender },
                            new ScopeClaim { ClaimCode = Core.Jwt.Constants.StandardResourceOwnerClaimNames.BirthDate },
                            new ScopeClaim { ClaimCode = Core.Jwt.Constants.StandardResourceOwnerClaimNames.ZoneInfo },
                            new ScopeClaim { ClaimCode = Core.Jwt.Constants.StandardResourceOwnerClaimNames.Locale },
                            new ScopeClaim { ClaimCode = Core.Jwt.Constants.StandardResourceOwnerClaimNames.UpdatedAt }
                        },
                        Type = ScopeType.ResourceOwner,
                        IsDisplayedInConsent = true
                    },
                    new Scope
                    {
                        Name = "email",
                        IsExposed = true,
                        IsOpenIdScope = true,
                        IsDisplayedInConsent = true,
                        Description = "Access to the email",
                        ScopeClaims = new List<ScopeClaim>
                        {
                            new ScopeClaim { ClaimCode = Core.Jwt.Constants.StandardResourceOwnerClaimNames.Email },
                            new ScopeClaim { ClaimCode = Core.Jwt.Constants.StandardResourceOwnerClaimNames.EmailVerified }
                        },
                        Type = ScopeType.ResourceOwner
                    },
                    new Scope
                    {
                        Name = "address",
                        IsExposed = true,
                        IsOpenIdScope = true,
                        IsDisplayedInConsent = true,
                        Description = "Access to the address",
                        ScopeClaims = new List<ScopeClaim>
                        {
                            new ScopeClaim { ClaimCode = Core.Jwt.Constants.StandardResourceOwnerClaimNames.Address }
                        },
                        Type = ScopeType.ResourceOwner
                    },
                    new Scope
                    {
                        Name = "phone",
                        IsExposed = true,
                        IsOpenIdScope = true,
                        IsDisplayedInConsent = true,
                        Description = "Access to the phone",
                        ScopeClaims = new List<ScopeClaim>
                        {
                            new ScopeClaim { ClaimCode = Core.Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumber },
                            new ScopeClaim { ClaimCode = Core.Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumberVerified }
                        },
                        Type = ScopeType.ResourceOwner
                    },
                    new Scope
                    {
                        Name = "role",
                        IsExposed = true,
                        IsOpenIdScope = false,
                        IsDisplayedInConsent = true,
                        Description = "Access to your roles",
                        ScopeClaims = new List<ScopeClaim>
                        {
                            new ScopeClaim { ClaimCode = Core.Jwt.Constants.StandardResourceOwnerClaimNames.Role }
                        },
                        Type = ScopeType.ResourceOwner
                    },
                    // Scopes needed by UMA solution
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
                    },
                    // Scopes needed to manage the openid assets
                    new Scope
                    {
                        Name = "openid_manager",
                        Description = "Access to the OpenId Manager",
                        IsOpenIdScope = false,
                        IsDisplayedInConsent = true,
                        Type = ScopeType.ProtectedApi
                    },
                    // Scopes needed by the configuration API
                    new Scope
                    {
                        Name = "manage_configuration",
                        Description = "Manage configuration",
                        IsOpenIdScope = false,
                        IsDisplayedInConsent = true,
                        Type = ScopeType.ProtectedApi
                    },
                    new Scope
                    {
                        Name = "display_configuration",
                        Description = "Display configuration",
                        IsOpenIdScope = false,
                        IsDisplayedInConsent = true,
                        Type = ScopeType.ProtectedApi
                    },
                    // Scope needed by the visual studio extension
                    new Scope
                    {
                        Name = "website_api",
                        Description = "Access to the website api",
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
                        Code = Constants.StandardTranslationCodes.ApplicationWouldLikeToCode,
                        Value = "the client {0} would like to access"
                    },
                    new Translation
                    {
                        LanguageTag = "en",
                        Code = Constants.StandardTranslationCodes.IndividualClaimsCode,
                        Value = "individual claims"
                    },
                    new Translation
                    {
                        LanguageTag = "en",
                        Code = Constants.StandardTranslationCodes.LoginCode,
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
                        Code = Constants.StandardTranslationCodes.UserNameCode,
                        Value = "Username"
                    },
                    new Translation
                    {
                        LanguageTag = "en",
                        Code = Constants.StandardTranslationCodes.ConfirmCode,
                        Value = "Confirm"
                    },
                    new Translation
                    {
                        LanguageTag = "en",
                        Code = Constants.StandardTranslationCodes.CancelCode,
                        Value = "Cancel"
                    },
                    new Translation
                    {
                        LanguageTag = "en",
                        Code = Constants.StandardTranslationCodes.LoginLocalAccount,
                        Value = "Login with your local account"
                    },
                    new Translation
                    {
                        LanguageTag = "en",
                        Code = Constants.StandardTranslationCodes.LoginExternalAccount,
                        Value = "Login with your external account"
                    },
                    new Translation
                    {
                        LanguageTag = "en",
                        Code = Constants.StandardTranslationCodes.LinkToThePolicy,
                        Value = "policy"
                    },
                    new Translation
                    {
                        LanguageTag = "en",
                        Code = Constants.StandardTranslationCodes.Tos,
                        Value = "Terms of Service"
                    },
                    // Swedish
                    new Translation
                    {
                        LanguageTag = "se",
                        Code = Constants.StandardTranslationCodes.ApplicationWouldLikeToCode,
                        Value = "tillämpning {0} skulle vilja:"
                    },
                    new Translation
                    {
                        LanguageTag = "se",
                        Code = Constants.StandardTranslationCodes.IndividualClaimsCode,
                        Value = "enskilda anspråk"
                    },
                    new Translation
                    {
                        LanguageTag = "se",
                        Code = Constants.StandardTranslationCodes.LoginCode,
                        Value = "Logga in"
                    },
                    new Translation
                    {
                        LanguageTag = "se",
                        Code = Constants.StandardTranslationCodes.PasswordCode,
                        Value = "Lösenord"
                    },
                    new Translation
                    {
                        LanguageTag = "se",
                        Code = Constants.StandardTranslationCodes.UserNameCode,
                        Value = "Användarnamn"
                    },
                    new Translation
                    {
                        LanguageTag = "se",
                        Code = Constants.StandardTranslationCodes.ConfirmCode,
                        Value = "bekräfta"
                    },
                    new Translation
                    {
                        LanguageTag = "se",
                        Code = Constants.StandardTranslationCodes.CancelCode,
                        Value = "annullera"
                    },
                    // French                
                    new Translation
                    {
                        LanguageTag = "fr",
                        Code = Constants.StandardTranslationCodes.ApplicationWouldLikeToCode,
                        Value = "L'application veut accéder à:"
                    },
                    new Translation
                    {
                        LanguageTag = "fr",
                        Code = Constants.StandardTranslationCodes.IndividualClaimsCode,
                        Value = "Les claims"
                    },
                    new Translation
                    {
                        LanguageTag = "fr",
                        Code = Constants.StandardTranslationCodes.LoginCode,
                        Value = "S'authentifier"
                    },
                    new Translation
                    {
                        LanguageTag = "fr",
                        Code = Constants.StandardTranslationCodes.PasswordCode,
                        Value = "Mot de passe"
                    },
                    new Translation
                    {
                        LanguageTag = "fr",
                        Code = Constants.StandardTranslationCodes.UserNameCode,
                        Value = "Nom d'utilisateur"
                    },
                    new Translation
                    {
                        LanguageTag = "fr",
                        Code = Constants.StandardTranslationCodes.ConfirmCode,
                        Value = "confirmer"
                    },
                    new Translation
                    {
                        LanguageTag = "fr",
                        Code = Constants.StandardTranslationCodes.CancelCode,
                        Value = "annuler"
                    }
                });
            }
        }

        private static void InsertResourceOwners(SimpleIdentityServerContext context)
        {
            if (!context.ResourceOwners.Any())
            {
                context.ResourceOwners.AddRange(new[]
                {
                    new ResourceOwner
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = "administrator",
                        Address = new Address
                        {
                            Id = Guid.NewGuid().ToString(),
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
                        Password = "5E884898DA28047151D0E56F8DC6292773603D0D6AABBDD62A11EF721D1542D8",
                        ResourceOwnerRoles = new List<ResourceOwnerRole>
                        {
                            new ResourceOwnerRole
                            {
                                RoleName = "administrator"
                            }
                        },
                        IsLocalAccount = true
                    }
                });
            }
        }

        private static void InsertJsonWebKeys(SimpleIdentityServerContext context)
        {
            if (!context.JsonWebKeys.Any())
            {
                var serializedRsa = string.Empty;
#if NET46
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
                    new JsonWebKey
                    {
                        Alg = AllAlg.RS256,
                        KeyOps = "0,1",
                        Kid = "1",
                        Kty = KeyType.RSA,
                        Use = Use.Sig,
                        SerializedKey = serializedRsa,
                    },
                    new JsonWebKey
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

        private static void InsertClients(SimpleIdentityServerContext context)
        {
            if (!context.Clients.Any())
            {
                context.Clients.AddRange(new[]
                {
                    // Configuration API : needs to interact with the introspection endpoint.
                    new Client
                    {
                        ClientId = "Configuration",
                        ClientSecret = "Configuration",
                        ClientName = "Configuration API",
                        TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.client_secret_post,
                        LogoUri = "http://img.over-blog-kiwi.com/1/47/73/14/20150513/ob_06dc4f_chiot-shiba-inu-a-vendre-prix-2015.jpg",
                        PolicyUri = "http://openid.net",
                        TosUri = "http://openid.net",
                        ApplicationType = ApplicationTypes.web
                    },
                    // Open Id manager API : needs to interact with the introspection endpoint.
                    new Client
                    {
                        ClientId = "OpenIdManager",
                        ClientSecret = "OpenIdManager",
                        ClientName = "OpenId Manager API",
                        TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.client_secret_post,
                        LogoUri = "http://img.over-blog-kiwi.com/1/47/73/14/20150513/ob_06dc4f_chiot-shiba-inu-a-vendre-prix-2015.jpg",
                        PolicyUri = "http://openid.net",
                        TosUri = "http://openid.net",
                        ApplicationType = ApplicationTypes.web
                    },
                    // UMA API : needs to interact with the introspection endpoint.
                    new Client
                    {
                        ClientId = "Uma",
                        ClientSecret = "Uma",
                        ClientName = "UMA API",
                        TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.client_secret_post,
                        LogoUri = "http://img.over-blog-kiwi.com/1/47/73/14/20150513/ob_06dc4f_chiot-shiba-inu-a-vendre-prix-2015.jpg",
                        PolicyUri = "http://openid.net",
                        TosUri = "http://openid.net",
                        ApplicationType = ApplicationTypes.web
                    },
                    // Anonymous client
                    new Client
                    {
                        ClientId = Constants.AnonymousClientId,
                        ClientName = "Anonymous client",
                        ClientSecret = "Anonymous",
                        TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.client_secret_post,
                        LogoUri = "http://img.over-blog-kiwi.com/1/47/73/14/20150513/ob_06dc4f_chiot-shiba-inu-a-vendre-prix-2015.jpg",
                        PolicyUri = "http://openid.net",
                        TosUri = "http://openid.net",
                        ClientScopes = new List<ClientScope>
                        {
                            new ClientScope
                            {
                                ScopeName = "openid"
                            },
                            new ClientScope
                            {
                                ScopeName = "role"
                            },
                            new ClientScope
                            {
                                ScopeName = "profile"
                            }
                        },
                        GrantTypes = "4",
                        ResponseTypes = "0,1,2",
                        IdTokenSignedResponseAlg = "RS256",
                        RedirectionUrls = "http://localhost:4200/callback",
                        ApplicationType = ApplicationTypes.web
                    },
                    // Manager website API
                    new Client
                    {
                        ClientId = "ManagerWebSiteApi",
                        ClientName = "Manager website API",
                        ClientSecret = "ManagerWebSiteApi",
                        TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.client_secret_post,
                        LogoUri = "http://img.over-blog-kiwi.com/1/47/73/14/20150513/ob_06dc4f_chiot-shiba-inu-a-vendre-prix-2015.jpg",
                        PolicyUri = "http://openid.net",
                        TosUri = "http://openid.net",
                        ClientScopes = new List<ClientScope>
                        {
                            new ClientScope
                            {
                                ScopeName = "openid"
                            },
                            new ClientScope
                            {
                                ScopeName = "uma_protection"
                            },
                            new ClientScope
                            {
                                ScopeName = "uma_authorization"
                            },
                            new ClientScope
                            {
                                ScopeName = "openid_manager"
                            },
                            new ClientScope
                            {
                                ScopeName = "manage_configuration"
                            },
                            new ClientScope
                            {
                                ScopeName = "display_configuration"
                            }
                        },
                        GrantTypes = "3",
                        ResponseTypes = "1",
                        IdTokenSignedResponseAlg = "RS256",
                        ApplicationType = ApplicationTypes.web
                    },
                    // Visual studio extension
                    new Client
                    {
                        ClientId = "VisualStudioExtension",
                        ClientName = "VisualStudioExtension",
                        ClientSecret = "VisualStudioExtension",
                        TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.client_secret_post,
                        LogoUri = "http://img.over-blog-kiwi.com/1/47/73/14/20150513/ob_06dc4f_chiot-shiba-inu-a-vendre-prix-2015.jpg",
                        PolicyUri = "http://openid.net",
                        TosUri = "http://openid.net",
                        ClientScopes = new List<ClientScope>
                        {
                            new ClientScope
                            {
                                ScopeName = "website_api"
                            }
                        },
                        GrantTypes = "3",
                        ResponseTypes = "1",
                        ApplicationType = ApplicationTypes.native

                    },
                    // SimpleIdentity server : needs to interact with the configuration server to retrieve his configuration
                    new Client
                    {
                        ClientId = "SimpleIdentityServer",
                        ClientSecret = "SimpleIdentityServer",
                        ClientName = "Simple Identity Server",
                        TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.client_secret_post,
                        LogoUri = "http://img.over-blog-kiwi.com/1/47/73/14/20150513/ob_06dc4f_chiot-shiba-inu-a-vendre-prix-2015.jpg",
                        PolicyUri = "http://openid.net",
                        ClientScopes = new List<ClientScope>
                        {
                            new ClientScope
                            {
                                ScopeName = "display_configuration"
                            }
                        },
                        GrantTypes = "3",
                        ResponseTypes = "1",
                        IdTokenSignedResponseAlg = "RS256",
                        ApplicationType = ApplicationTypes.web
                    },
                    // Manager website 
                    new Client
                    {
                        ClientId = "SampleClient",
                        ClientName = "Sample client",
                        ClientSecret = "SampleClient",
                        TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.client_secret_post,
                        LogoUri = "http://img.over-blog-kiwi.com/1/47/73/14/20150513/ob_06dc4f_chiot-shiba-inu-a-vendre-prix-2015.jpg",
                        ClientScopes = new List<ClientScope>
                        {
                            new ClientScope
                            {
                                ScopeName = "openid"
                            },
                            new ClientScope
                            {
                                ScopeName = "uma_protection"
                            },
                            new ClientScope
                            {
                                ScopeName = "uma_authorization"
                            }
                        },
                        GrantTypes = "3",
                        ResponseTypes = "1",
                        IdTokenSignedResponseAlg = "RS256",
                        ApplicationType = ApplicationTypes.web
                    },
                    // Simple Identity server test client
                    new Client
                    {
                        ClientId = "SimpleIdServerClient",
                        ClientName = "Simple Identity Server Client",
                        ClientSecret = "SimpleIdServerClient",
                        TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.client_secret_post,
                        LogoUri = "http://img.over-blog-kiwi.com/1/47/73/14/20150513/ob_06dc4f_chiot-shiba-inu-a-vendre-prix-2015.jpg",
                        ClientScopes = new List<ClientScope>
                        {
                            new ClientScope
                            {
                                ScopeName = "openid"
                            },
                            new ClientScope
                            {
                                ScopeName = "role"
                            },
                            new ClientScope
                            {
                                ScopeName = "profile"
                            }
                        },
                        GrantTypes = "1",
                        ResponseTypes = "0,1,2",
                        IdTokenSignedResponseAlg = "RS256",
                        RedirectionUrls = "https://localhost:5443/User/Callback",
                        ApplicationType = ApplicationTypes.web
                    }
                });
            }
        }

        #endregion
    }
}
