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
            if (context.AllMigrationsApplied())
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
                    new Scope
                    {
                        Name = "SimpleIdentityServerManager:GetClients",
                        Description = "Get all the clients",
                        IsOpenIdScope = false,
                        IsDisplayedInConsent = true,
                        Type = ScopeType.ProtectedApi
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
                    }
                });
            }
        }

        private static void InsertTranslations(SimpleIdentityServerContext context)
        {
            if (!context.Translations.Any())
            {
                context.Translations.AddRange(new[] {
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
        }

        private static void InsertResourceOwners(SimpleIdentityServerContext context)
        {
            if (!context.ResourceOwners.Any())
            {
                context.ResourceOwners.AddRange(new[]
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
                        Password = "5E884898DA28047151D0E56F8DC6292773603D0D6AABBDD62A11EF721D1542D8",
                        ResourceOwnerRoles = new List<ResourceOwnerRole>
                        {
                            new ResourceOwnerRole
                            {
                                RoleName = "administrator"
                            }
                        }
                    }
                });
            }
        }

        private static void InsertJsonWebKeys(SimpleIdentityServerContext context)
        {
            if (!context.JsonWebKeys.Any())
            {
                var serializedRsa = string.Empty;
#if DNXCORE50
                using (var rsa = new RSAOpenSsl())
                {
                    serializedRsa = rsa.ToXmlString(true);
                }
#endif
#if DNX451
                using (var provider = new RSACryptoServiceProvider())
                {
                    serializedRsa = provider.ToXmlString(true);
                }
#endif

                    context.JsonWebKeys.AddRange(new[]
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

        private static void InsertClients(SimpleIdentityServerContext context)
        {
            if (!context.Clients.Any())
            {
                context.Clients.AddRange(new[]
                {
                    new Client
                    {
                        ClientId = "IdentityServerManager",
                        ClientName = "Identity server manager",
                        ClientSecret = "IdentityServerManager",
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
                            }
                        },
                        GrantTypes = "0,1",
                        ResponseTypes = "0,1,2",
                        IdTokenSignedResponseAlg = "RS256",
                        RedirectionUrls = "http://localhost:5002/callback,http://localhost:5001/swagger/ui/o2c.html"
                    },
                    new Client
                    {
                        ClientId = "UmaResourceServer",
                        ClientName = "Resource server sample",
                        ClientSecret = "UmaResourceServer",
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
                            }
                        },
                        GrantTypes = "0,1",
                        ResponseTypes = "0,1,2",
                        IdTokenSignedResponseAlg = "RS256",
                        RedirectionUrls = "http://localhost:5007/swagger/ui/o2c.html"
                    }
                });
            }
        }

#endregion
    }
}
