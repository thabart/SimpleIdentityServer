using SimpleIdentityServer.EF;
using SimpleIdentityServer.EF.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SimpleIdentityServer.Core;
using SimpleIdentityServer.Core.Extensions;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using SimpleIdentityServer.Core.Common.Extensions;

namespace SimpleIdentityServer.Startup.Extensions
{
    public static class SimpleIdentityServerContextExtensions
    {
        public static void EnsureSeedData(this SimpleIdentityServerContext context)
        {
            InsertClaims(context);
            InsertScopes(context);
            InsertTranslations(context);
            InsertResourceOwners(context);
            InsertJsonWebKeys(context);
            InsertClients(context);
            context.SaveChanges();
        }

        private static void InsertClaims(SimpleIdentityServerContext context)
        {
            if (!context.Claims.Any())
            {
                context.Claims.AddRange(new[] {
                    new Claim { Code = Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject, IsIdentifier = true },
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
                    new Claim { Code = Core.Jwt.Constants.StandardResourceOwnerClaimNames.Role },
                    new Claim { Code = Core.Jwt.Constants.StandardResourceOwnerClaimNames.ScimId },
                    new Claim { Code = Core.Jwt.Constants.StandardResourceOwnerClaimNames.ScimLocation }
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
                        Type = ScopeType.ProtectedApi,
                        UpdateDateTime = DateTime.UtcNow,
                        CreateDateTime = DateTime.UtcNow
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
                        IsDisplayedInConsent = true,
                        UpdateDateTime = DateTime.UtcNow,
                        CreateDateTime = DateTime.UtcNow
                    },
                    new Scope
                    {
                        Name = "scim",
                        IsExposed = true,
                        IsOpenIdScope = true,
                        Description = "Access to the scim",
                        ScopeClaims = new List<ScopeClaim>
                        {
                            new ScopeClaim { ClaimCode = Core.Jwt.Constants.StandardResourceOwnerClaimNames.ScimId },
                            new ScopeClaim { ClaimCode = Core.Jwt.Constants.StandardResourceOwnerClaimNames.ScimLocation }
                        },
                        Type = ScopeType.ResourceOwner,
                        IsDisplayedInConsent = true,
                        UpdateDateTime = DateTime.UtcNow,
                        CreateDateTime = DateTime.UtcNow
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
                        Type = ScopeType.ResourceOwner,
                        UpdateDateTime = DateTime.UtcNow,
                        CreateDateTime = DateTime.UtcNow
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
                        Type = ScopeType.ResourceOwner,
                        UpdateDateTime = DateTime.UtcNow,
                        CreateDateTime = DateTime.UtcNow
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
                        Type = ScopeType.ResourceOwner,
                        UpdateDateTime = DateTime.UtcNow,
                        CreateDateTime = DateTime.UtcNow
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
                        Type = ScopeType.ResourceOwner,
                        UpdateDateTime = DateTime.UtcNow,
                        CreateDateTime = DateTime.UtcNow
                    },
                    // Scopes needed by UMA solution
                    new Scope
                    {
                        Name = "uma_protection",
                        Description = "Access to UMA permission, resource set & token introspection endpoints",
                        IsOpenIdScope = false,
                        IsDisplayedInConsent = true,
                        Type = ScopeType.ProtectedApi,
                        UpdateDateTime = DateTime.UtcNow,
                        CreateDateTime = DateTime.UtcNow
                    },
                    new Scope
                    {
                        Name = "uma_authorization",
                        Description = "Access to the UMA authorization endpoint",
                        IsOpenIdScope = false,
                        IsDisplayedInConsent = true,
                        Type = ScopeType.ProtectedApi,
                        UpdateDateTime = DateTime.UtcNow,
                        CreateDateTime = DateTime.UtcNow
                    },
                    new Scope
                    {
                        Name = "uma",
                        Description = "UMA",
                        IsOpenIdScope = false,
                        IsDisplayedInConsent = true,
                        Type = ScopeType.ProtectedApi,
                        UpdateDateTime = DateTime.UtcNow,
                        CreateDateTime = DateTime.UtcNow
                    },
                    // Scopes needed to manage the openid assets
                    new Scope
                    {
                        Name = "openid_manager",
                        Description = "Access to the OpenId Manager",
                        IsOpenIdScope = false,
                        IsDisplayedInConsent = true,
                        Type = ScopeType.ProtectedApi,
                        UpdateDateTime = DateTime.UtcNow,
                        CreateDateTime = DateTime.UtcNow
                    },
                    // Scopes needed by the configuration API
                    new Scope
                    {
                        Name = "manage_configuration",
                        Description = "Manage configuration",
                        IsOpenIdScope = false,
                        IsDisplayedInConsent = true,
                        Type = ScopeType.ProtectedApi,
                        UpdateDateTime = DateTime.UtcNow,
                        CreateDateTime = DateTime.UtcNow
                    },
                    new Scope
                    {
                        Name = "display_configuration",
                        Description = "Display configuration",
                        IsOpenIdScope = false,
                        IsDisplayedInConsent = true,
                        Type = ScopeType.ProtectedApi,
                        UpdateDateTime = DateTime.UtcNow,
                        CreateDateTime = DateTime.UtcNow
                    },
                    new Scope
                    {
                        Name = "configuration",
                        Description = "Configuration",
                        IsOpenIdScope = false,
                        IsDisplayedInConsent = true,
                        Type = ScopeType.ProtectedApi,
                        UpdateDateTime = DateTime.UtcNow,
                        CreateDateTime = DateTime.UtcNow
                    },
                    // Scope needed by the visual studio extension
                    new Scope
                    {
                        Name = "website_api",
                        Description = "Access to the website api",
                        IsOpenIdScope = false,
                        IsDisplayedInConsent = true,
                        Type = ScopeType.ProtectedApi,
                        UpdateDateTime = DateTime.UtcNow,
                        CreateDateTime = DateTime.UtcNow
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
                        Code = Core.Constants.StandardTranslationCodes.NameCode,
                        Value = "Logga in"
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
                        Code = Core.Constants.StandardTranslationCodes.YourName,
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
                        Id = "administrator",
                        Claims = new List<ResourceOwnerClaim>
                        {
                            new ResourceOwnerClaim
                            {
                                Id = Guid.NewGuid().ToString(),
                                ClaimCode = Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject,
                                Value = "administrator"
                            },
                            new ResourceOwnerClaim
                            {
                                Id = Guid.NewGuid().ToString(),
                                ClaimCode = Core.Jwt.Constants.StandardResourceOwnerClaimNames.Role,
                                Value = "administrator"
                            },
                            new ResourceOwnerClaim
                            {
                                Id = Guid.NewGuid().ToString(),
                                ClaimCode = Core.Jwt.Constants.StandardResourceOwnerClaimNames.Address,
                                Value = "{ country : 'france' }"
                            },
                            new ResourceOwnerClaim
                            {
                                Id = Guid.NewGuid().ToString(),
                                ClaimCode = Core.Jwt.Constants.StandardResourceOwnerClaimNames.BirthDate,
                                Value = "1989-10-07"
                            },
                            new ResourceOwnerClaim
                            {
                                Id = Guid.NewGuid().ToString(),
                                ClaimCode = Core.Jwt.Constants.StandardResourceOwnerClaimNames.Email,
                                Value = "habarthierry@hotmail.fr"
                            },
                            new ResourceOwnerClaim
                            {
                                Id = Guid.NewGuid().ToString(),
                                ClaimCode = Core.Jwt.Constants.StandardResourceOwnerClaimNames.EmailVerified,
                                Value = "true"
                            },
                            new ResourceOwnerClaim
                            {
                                Id = Guid.NewGuid().ToString(),
                                ClaimCode = Core.Jwt.Constants.StandardResourceOwnerClaimNames.FamilyName,
                                Value = "habart"
                            },
                            new ResourceOwnerClaim
                            {
                                Id = Guid.NewGuid().ToString(),
                                ClaimCode = Core.Jwt.Constants.StandardResourceOwnerClaimNames.Gender,
                                Value = "M"
                            },
                            new ResourceOwnerClaim
                            {
                                Id = Guid.NewGuid().ToString(),
                                ClaimCode = Core.Jwt.Constants.StandardResourceOwnerClaimNames.GivenName,
                                Value = "Habart Thierry"
                            },
                            new ResourceOwnerClaim
                            {
                                Id = Guid.NewGuid().ToString(),
                                ClaimCode = Core.Jwt.Constants.StandardResourceOwnerClaimNames.Locale,
                                Value = "fr-FR"
                            },
                            new ResourceOwnerClaim
                            {
                                Id = Guid.NewGuid().ToString(),
                                ClaimCode = Core.Jwt.Constants.StandardResourceOwnerClaimNames.MiddleName,
                                Value = "Thierry"
                            },
                            new ResourceOwnerClaim
                            {
                                Id = Guid.NewGuid().ToString(),
                                ClaimCode = Core.Jwt.Constants.StandardResourceOwnerClaimNames.NickName,
                                Value = "Titi"
                            },
                            new ResourceOwnerClaim
                            {
                                Id = Guid.NewGuid().ToString(),
                                ClaimCode = Core.Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumber,
                                Value = "+32485350536"
                            },
                            new ResourceOwnerClaim
                            {
                                Id = Guid.NewGuid().ToString(),
                                ClaimCode = Core.Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumberVerified,
                                Value = "true"
                            },
                            new ResourceOwnerClaim
                            {
                                Id = Guid.NewGuid().ToString(),
                                ClaimCode = Core.Jwt.Constants.StandardResourceOwnerClaimNames.Picture,
                                Value = "https://upload.wikimedia.org/wikipedia/commons/thumb/5/58/Shiba_inu_taiki.jpg/220px-Shiba_inu_taiki.jpg"
                            },
                            new ResourceOwnerClaim
                            {
                                Id = Guid.NewGuid().ToString(),
                                ClaimCode = Core.Jwt.Constants.StandardResourceOwnerClaimNames.PreferredUserName,
                                Value = "Thierry"
                            },
                            new ResourceOwnerClaim
                            {
                                Id = Guid.NewGuid().ToString(),
                                ClaimCode = Core.Jwt.Constants.StandardResourceOwnerClaimNames.Profile,
                                Value = "http://localhost/profile"
                            },
                            new ResourceOwnerClaim
                            {
                                Id = Guid.NewGuid().ToString(),
                                ClaimCode = Core.Jwt.Constants.StandardResourceOwnerClaimNames.UpdatedAt,
                                Value = DateTime.Now.ConvertToUnixTimestamp().ToString()
                            },
                            new ResourceOwnerClaim
                            {
                                Id = Guid.NewGuid().ToString(),
                                ClaimCode = Core.Jwt.Constants.StandardResourceOwnerClaimNames.WebSite,
                                Value = "https://github.com/thabart"
                            },
                            new ResourceOwnerClaim
                            {
                                Id = Guid.NewGuid().ToString(),
                                ClaimCode = Core.Jwt.Constants.StandardResourceOwnerClaimNames.ZoneInfo,
                                Value = "Europe/Paris"
                            },
                            new ResourceOwnerClaim
                            {
                                Id = Guid.NewGuid().ToString(),
                                ClaimCode = Core.Jwt.Constants.StandardResourceOwnerClaimNames.ScimId,
                                Value = "id"
                            },
                            new ResourceOwnerClaim
                            {
                                Id = Guid.NewGuid().ToString(),
                                ClaimCode = Core.Jwt.Constants.StandardResourceOwnerClaimNames.ScimLocation,
                                Value = "http://localhost:5555/Users/id"
                            }
                        },
                        Password = "5E884898DA28047151D0E56F8DC6292773603D0D6AABBDD62A11EF721D1542D8",
                        IsLocalAccount = true,
                        UpdateDateTime = DateTime.UtcNow,
                        CreateDateTime = DateTime.UtcNow
                    },
                    new ResourceOwner // Patient
                    {
                        Id = "patient",
                        Claims = new List<ResourceOwnerClaim>
                        {
                            new ResourceOwnerClaim
                            {
                                Id = Guid.NewGuid().ToString(),
                                ClaimCode = Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject,
                                Value = "patient"
                            },
                            new ResourceOwnerClaim
                            {
                                Id = Guid.NewGuid().ToString(),
                                ClaimCode = Core.Jwt.Constants.StandardResourceOwnerClaimNames.Role,
                                Value = "patient"
                            },
                            new ResourceOwnerClaim
                            {
                                Id = Guid.NewGuid().ToString(),
                                ClaimCode = Core.Jwt.Constants.StandardResourceOwnerClaimNames.Address,
                                Value = "{ country : 'france' }"
                            },
                            new ResourceOwnerClaim
                            {
                                Id = Guid.NewGuid().ToString(),
                                ClaimCode = Core.Jwt.Constants.StandardResourceOwnerClaimNames.BirthDate,
                                Value = "1989-10-07"
                            },
                            new ResourceOwnerClaim
                            {
                                Id = Guid.NewGuid().ToString(),
                                ClaimCode = Core.Jwt.Constants.StandardResourceOwnerClaimNames.Email,
                                Value = "patient@hotmail.fr"
                            },
                            new ResourceOwnerClaim
                            {
                                Id = Guid.NewGuid().ToString(),
                                ClaimCode = Core.Jwt.Constants.StandardResourceOwnerClaimNames.EmailVerified,
                                Value = "true"
                            },
                            new ResourceOwnerClaim
                            {
                                Id = Guid.NewGuid().ToString(),
                                ClaimCode = Core.Jwt.Constants.StandardResourceOwnerClaimNames.FamilyName,
                                Value = "patient"
                            },
                            new ResourceOwnerClaim
                            {
                                Id = Guid.NewGuid().ToString(),
                                ClaimCode = Core.Jwt.Constants.StandardResourceOwnerClaimNames.Gender,
                                Value = "M"
                            },
                            new ResourceOwnerClaim
                            {
                                Id = Guid.NewGuid().ToString(),
                                ClaimCode = Core.Jwt.Constants.StandardResourceOwnerClaimNames.GivenName,
                                Value = "Mr. Patient"
                            },
                            new ResourceOwnerClaim
                            {
                                Id = Guid.NewGuid().ToString(),
                                ClaimCode = Core.Jwt.Constants.StandardResourceOwnerClaimNames.Locale,
                                Value = "fr-FR"
                            },
                            new ResourceOwnerClaim
                            {
                                Id = Guid.NewGuid().ToString(),
                                ClaimCode = Core.Jwt.Constants.StandardResourceOwnerClaimNames.MiddleName,
                                Value = "Patient"
                            },
                            new ResourceOwnerClaim
                            {
                                Id = Guid.NewGuid().ToString(),
                                ClaimCode = Core.Jwt.Constants.StandardResourceOwnerClaimNames.NickName,
                                Value = "Patient"
                            },
                            new ResourceOwnerClaim
                            {
                                Id = Guid.NewGuid().ToString(),
                                ClaimCode = Core.Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumber,
                                Value = "+32485350536"
                            },
                            new ResourceOwnerClaim
                            {
                                Id = Guid.NewGuid().ToString(),
                                ClaimCode = Core.Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumberVerified,
                                Value = "true"
                            },
                            new ResourceOwnerClaim
                            {
                                Id = Guid.NewGuid().ToString(),
                                ClaimCode = Core.Jwt.Constants.StandardResourceOwnerClaimNames.Picture,
                                Value = "https://upload.wikimedia.org/wikipedia/commons/thumb/5/58/Shiba_inu_taiki.jpg/220px-Shiba_inu_taiki.jpg"
                            },
                            new ResourceOwnerClaim
                            {
                                Id = Guid.NewGuid().ToString(),
                                ClaimCode = Core.Jwt.Constants.StandardResourceOwnerClaimNames.PreferredUserName,
                                Value = "Patient"
                            },
                            new ResourceOwnerClaim
                            {
                                Id = Guid.NewGuid().ToString(),
                                ClaimCode = Core.Jwt.Constants.StandardResourceOwnerClaimNames.Profile,
                                Value = "http://localhost/profile"
                            },
                            new ResourceOwnerClaim
                            {
                                Id = Guid.NewGuid().ToString(),
                                ClaimCode = Core.Jwt.Constants.StandardResourceOwnerClaimNames.UpdatedAt,
                                Value = DateTime.Now.ConvertToUnixTimestamp().ToString()
                            },
                            new ResourceOwnerClaim
                            {
                                Id = Guid.NewGuid().ToString(),
                                ClaimCode = Core.Jwt.Constants.StandardResourceOwnerClaimNames.ZoneInfo,
                                Value = "Europe/Paris"
                            },
                            new ResourceOwnerClaim
                            {
                                Id = Guid.NewGuid().ToString(),
                                ClaimCode = Core.Jwt.Constants.StandardResourceOwnerClaimNames.ScimId,
                                Value = "id"
                            },
                            new ResourceOwnerClaim
                            {
                                Id = Guid.NewGuid().ToString(),
                                ClaimCode = Core.Jwt.Constants.StandardResourceOwnerClaimNames.ScimLocation,
                                Value = "http://localhost:5555/Users/id"
                            }
                        },
                        Password = "5E884898DA28047151D0E56F8DC6292773603D0D6AABBDD62A11EF721D1542D8",
                        IsLocalAccount = true,
                        UpdateDateTime = DateTime.UtcNow,
                        CreateDateTime = DateTime.UtcNow
                    },
                    new ResourceOwner // doctor
                    {
                        Id = "doctor",
                        Claims = new List<ResourceOwnerClaim>
                        {
                            new ResourceOwnerClaim
                            {
                                Id = Guid.NewGuid().ToString(),
                                ClaimCode = Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject,
                                Value = "doctor"
                            },
                            new ResourceOwnerClaim
                            {
                                Id = Guid.NewGuid().ToString(),
                                ClaimCode = Core.Jwt.Constants.StandardResourceOwnerClaimNames.Role,
                                Value = "doctor"
                            },
                            new ResourceOwnerClaim
                            {
                                Id = Guid.NewGuid().ToString(),
                                ClaimCode = Core.Jwt.Constants.StandardResourceOwnerClaimNames.Address,
                                Value = "{ country : 'france' }"
                            },
                            new ResourceOwnerClaim
                            {
                                Id = Guid.NewGuid().ToString(),
                                ClaimCode = Core.Jwt.Constants.StandardResourceOwnerClaimNames.BirthDate,
                                Value = "1989-10-07"
                            },
                            new ResourceOwnerClaim
                            {
                                Id = Guid.NewGuid().ToString(),
                                ClaimCode = Core.Jwt.Constants.StandardResourceOwnerClaimNames.Email,
                                Value = "doctor@hotmail.fr"
                            },
                            new ResourceOwnerClaim
                            {
                                Id = Guid.NewGuid().ToString(),
                                ClaimCode = Core.Jwt.Constants.StandardResourceOwnerClaimNames.EmailVerified,
                                Value = "true"
                            },
                            new ResourceOwnerClaim
                            {
                                Id = Guid.NewGuid().ToString(),
                                ClaimCode = Core.Jwt.Constants.StandardResourceOwnerClaimNames.FamilyName,
                                Value = "doctor"
                            },
                            new ResourceOwnerClaim
                            {
                                Id = Guid.NewGuid().ToString(),
                                ClaimCode = Core.Jwt.Constants.StandardResourceOwnerClaimNames.Gender,
                                Value = "M"
                            },
                            new ResourceOwnerClaim
                            {
                                Id = Guid.NewGuid().ToString(),
                                ClaimCode = Core.Jwt.Constants.StandardResourceOwnerClaimNames.GivenName,
                                Value = "Mr. Doctor"
                            },
                            new ResourceOwnerClaim
                            {
                                Id = Guid.NewGuid().ToString(),
                                ClaimCode = Core.Jwt.Constants.StandardResourceOwnerClaimNames.Locale,
                                Value = "fr-FR"
                            },
                            new ResourceOwnerClaim
                            {
                                Id = Guid.NewGuid().ToString(),
                                ClaimCode = Core.Jwt.Constants.StandardResourceOwnerClaimNames.MiddleName,
                                Value = "Doctor"
                            },
                            new ResourceOwnerClaim
                            {
                                Id = Guid.NewGuid().ToString(),
                                ClaimCode = Core.Jwt.Constants.StandardResourceOwnerClaimNames.NickName,
                                Value = "Doctor"
                            },
                            new ResourceOwnerClaim
                            {
                                Id = Guid.NewGuid().ToString(),
                                ClaimCode = Core.Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumber,
                                Value = "+32485350536"
                            },
                            new ResourceOwnerClaim
                            {
                                Id = Guid.NewGuid().ToString(),
                                ClaimCode = Core.Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumberVerified,
                                Value = "true"
                            },
                            new ResourceOwnerClaim
                            {
                                Id = Guid.NewGuid().ToString(),
                                ClaimCode = Core.Jwt.Constants.StandardResourceOwnerClaimNames.Picture,
                                Value = "https://upload.wikimedia.org/wikipedia/commons/thumb/5/58/Shiba_inu_taiki.jpg/220px-Shiba_inu_taiki.jpg"
                            },
                            new ResourceOwnerClaim
                            {
                                Id = Guid.NewGuid().ToString(),
                                ClaimCode = Core.Jwt.Constants.StandardResourceOwnerClaimNames.PreferredUserName,
                                Value = "Doctor"
                            },
                            new ResourceOwnerClaim
                            {
                                Id = Guid.NewGuid().ToString(),
                                ClaimCode = Core.Jwt.Constants.StandardResourceOwnerClaimNames.Profile,
                                Value = "http://localhost/profile"
                            },
                            new ResourceOwnerClaim
                            {
                                Id = Guid.NewGuid().ToString(),
                                ClaimCode = Core.Jwt.Constants.StandardResourceOwnerClaimNames.UpdatedAt,
                                Value = DateTime.Now.ConvertToUnixTimestamp().ToString()
                            },
                            new ResourceOwnerClaim
                            {
                                Id = Guid.NewGuid().ToString(),
                                ClaimCode = Core.Jwt.Constants.StandardResourceOwnerClaimNames.ZoneInfo,
                                Value = "Europe/Paris"
                            },
                            new ResourceOwnerClaim
                            {
                                Id = Guid.NewGuid().ToString(),
                                ClaimCode = Core.Jwt.Constants.StandardResourceOwnerClaimNames.ScimId,
                                Value = "id"
                            },
                            new ResourceOwnerClaim
                            {
                                Id = Guid.NewGuid().ToString(),
                                ClaimCode = Core.Jwt.Constants.StandardResourceOwnerClaimNames.ScimLocation,
                                Value = "http://localhost:5555/Users/id"
                            }
                        },
                        Password = "5E884898DA28047151D0E56F8DC6292773603D0D6AABBDD62A11EF721D1542D8",
                        IsLocalAccount = true,
                        UpdateDateTime = DateTime.UtcNow,
                        CreateDateTime = DateTime.UtcNow
                    }
                });
            }
        }

        private static void InsertJsonWebKeys(SimpleIdentityServerContext context)
        {
            if (!context.JsonWebKeys.Any())
            {
                var serializedRsa = string.Empty;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    using (var provider = new RSACryptoServiceProvider())
                    {
                        serializedRsa = provider.ToXmlStringNetCore(true);
                    }
                }
                else
                {
                    using (var rsa = new RSAOpenSsl())
                    {
                        serializedRsa = rsa.ToXmlStringNetCore(true);
                    }
                }

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
                    new EF.Models.Client
                    {
                        ClientId = "Configuration",
                        ClientSecrets = new List<ClientSecret>
                        {
                            new ClientSecret
                            {
                                Id = Guid.NewGuid().ToString(),
                                Type = SecretTypes.SharedSecret,
                                Value = "Configuration"
                            }
                        },
                        ClientName = "Configuration API",
                        TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.client_secret_post,
                        LogoUri = "http://img.over-blog-kiwi.com/1/47/73/14/20150513/ob_06dc4f_chiot-shiba-inu-a-vendre-prix-2015.jpg",
                        PolicyUri = "http://openid.net",
                        TosUri = "http://openid.net",
                        ApplicationType = ApplicationTypes.web,
                        RedirectionUrls = "https://configuration/callback",
                        UpdateDateTime = DateTime.UtcNow,
                        CreateDateTime = DateTime.UtcNow
                    },
                    // Open Id manager API : needs to interact with the introspection endpoint.
                    new EF.Models.Client
                    {
                        ClientId = "OpenIdManager",
                        ClientSecrets = new List<ClientSecret>
                        {
                            new ClientSecret
                            {
                                Id = Guid.NewGuid().ToString(),
                                Type = SecretTypes.SharedSecret,
                                Value = "OpenIdManager"
                            }
                        },
                        ClientName = "OpenId Manager API",
                        TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.client_secret_post,
                        LogoUri = "http://img.over-blog-kiwi.com/1/47/73/14/20150513/ob_06dc4f_chiot-shiba-inu-a-vendre-prix-2015.jpg",
                        PolicyUri = "http://openid.net",
                        TosUri = "http://openid.net",
                        ApplicationType = ApplicationTypes.web,
                        RedirectionUrls = "https://openidmanager/callback",
                        UpdateDateTime = DateTime.UtcNow,
                        CreateDateTime = DateTime.UtcNow
                    },
                    // UMA API : needs to interact with the introspection endpoint.
                    new EF.Models.Client
                    {
                        ClientId = "Uma",
                        ClientSecrets = new List<ClientSecret>
                        {
                            new ClientSecret
                            {
                                Id = Guid.NewGuid().ToString(),
                                Type = SecretTypes.SharedSecret,
                                Value = "Uma"
                            }
                        },
                        ClientName = "UMA API",
                        TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.client_secret_post,
                        LogoUri = "http://img.over-blog-kiwi.com/1/47/73/14/20150513/ob_06dc4f_chiot-shiba-inu-a-vendre-prix-2015.jpg",
                        PolicyUri = "http://openid.net",
                        TosUri = "http://openid.net",
                        ApplicationType = ApplicationTypes.web,
                        RedirectionUrls = "https://uma/callback",
                        UpdateDateTime = DateTime.UtcNow,
                        CreateDateTime = DateTime.UtcNow
                    },
                    // Anonymous client
                    new EF.Models.Client
                    {
                        ClientId = Core.Constants.AnonymousClientId,
                        ClientName = "Anonymous client",
                        ClientSecrets = new List<ClientSecret>
                        {
                            new ClientSecret
                            {
                                Id = Guid.NewGuid().ToString(),
                                Type = SecretTypes.SharedSecret,
                                Value = "Anonymous"
                            }
                        },
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
                        ApplicationType = ApplicationTypes.web,
                        RedirectionUrls = "https://localhost:4200/callback",
                        UpdateDateTime = DateTime.UtcNow,
                        CreateDateTime = DateTime.UtcNow
                    },
                    // Website
                    new EF.Models.Client
                    {
                        ClientId = "website",
                        ClientName = "Website",
                        ClientSecrets = new List<ClientSecret>
                        {
                            new ClientSecret
                            {
                                Id = Guid.NewGuid().ToString(),
                                Type = SecretTypes.SharedSecret,
                                Value = "website"
                            }
                        },
                        TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.client_secret_basic,
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
                            },
                            new ClientScope
                            {
                                ScopeName = "website_api"
                            }
                        },
                        GrantTypes = "4",
                        ResponseTypes = "0,1,2",
                        IdTokenSignedResponseAlg = "RS256",
                        ApplicationType = ApplicationTypes.web,
                        RedirectionUrls = "https://website/callback",
                        UpdateDateTime = DateTime.UtcNow,
                        CreateDateTime = DateTime.UtcNow
                    },
                    new EF.Models.Client // Medical website.
                    {
                        ClientId = "MedicalWebsite",
                        ClientName = "MedicalWebsite",
                        ClientSecrets = new List<ClientSecret>
                        {
                            new ClientSecret
                            {
                                Id = Guid.NewGuid().ToString(),
                                Type = SecretTypes.SharedSecret,
                                Value = "MedicalWebsite"
                            }
                        },
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
                            },
                            new ClientScope
                            {
                                ScopeName = "uma"
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
                                ScopeName = "website_api"
                            }
                        },
                        GrantTypes = "1,3,4",
                        ResponseTypes = "0,1,2",
                        IdTokenSignedResponseAlg = "RS256",
                        ApplicationType = ApplicationTypes.web,
                        RedirectionUrls = "https://localhost:5106/Authenticate/Callback",
                        UpdateDateTime = DateTime.UtcNow,
                        CreateDateTime = DateTime.UtcNow
                    },
                    // Manager website API
                    new EF.Models.Client
                    {
                        ClientId = "ManagerWebSiteApi",
                        ClientName = "Manager website API",
                        ClientSecrets = new List<ClientSecret>
                        {
                            new ClientSecret
                            {
                                Id = Guid.NewGuid().ToString(),
                                Type = SecretTypes.SharedSecret,
                                Value = "ManagerWebSiteApi"
                            }
                        },
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
                            },
                            new ClientScope
                            {
                                ScopeName = "configuration"
                            },
                            new ClientScope
                            {
                                ScopeName = "uma"
                            }
                        },
                        GrantTypes = "3",
                        ResponseTypes = "1",
                        IdTokenSignedResponseAlg = "RS256",
                        ApplicationType = ApplicationTypes.web,
                        RedirectionUrls = "https://websiteapi/callback",
                        UpdateDateTime = DateTime.UtcNow,
                        CreateDateTime = DateTime.UtcNow
                    },
                    // Resource manager website.
                    new EF.Models.Client
                    {
                        ClientId = "ResourceManagerClientId",
                        ClientName = "Resource manager website",
                        ClientSecrets = new List<ClientSecret>
                        {
                            new ClientSecret
                            {
                                Id = Guid.NewGuid().ToString(),
                                Type = SecretTypes.SharedSecret,
                                Value = "ResourceManagerClientId"
                            }
                        },
                        TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.client_secret_basic,
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
                            },
                            new ClientScope
                            {
                                ScopeName = "website_api"
                            }
                        },
                        GrantTypes = "1,4",
                        ResponseTypes = "0,1,2",
                        IdTokenSignedResponseAlg = "RS256",
                        ApplicationType = ApplicationTypes.web,
                        RedirectionUrls = "http://localhost:64950/callback",
                        PostLogoutRedirectUris = "http://localhost:64950/end_session",
                        UpdateDateTime = DateTime.UtcNow,
                        CreateDateTime = DateTime.UtcNow
                    },
                    // Visual studio extension
                    new EF.Models.Client
                    {
                        ClientId = "VisualStudioExtension",
                        ClientName = "VisualStudioExtension",
                        ClientSecrets = new List<ClientSecret>
                        {
                            new ClientSecret
                            {
                                Id = Guid.NewGuid().ToString(),
                                Type = SecretTypes.SharedSecret,
                                Value = "VisualStudioExtension"
                            }
                        },
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
                        ApplicationType = ApplicationTypes.native,
                        UpdateDateTime = DateTime.UtcNow,
                        CreateDateTime = DateTime.UtcNow

                    },
                    // SimpleIdentity server : needs to interact with the configuration server to retrieve his configuration
                    new EF.Models.Client
                    {
                        ClientId = "SimpleIdentityServer",
                        ClientSecrets = new List<ClientSecret>
                        {
                            new ClientSecret
                            {
                                Id = Guid.NewGuid().ToString(),
                                Type = SecretTypes.SharedSecret,
                                Value = "SimpleIdentityServer"
                            }
                        },
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
                        ApplicationType = ApplicationTypes.web,
                        RedirectionUrls = "https://simpleidserver/callback",
                        UpdateDateTime = DateTime.UtcNow,
                        CreateDateTime = DateTime.UtcNow
                    },
                    // Manager website 
                    new EF.Models.Client
                    {
                        ClientId = "SampleClient",
                        ClientName = "Sample client",
                        ClientSecrets = new List<ClientSecret>
                        {
                            new ClientSecret
                            {
                                Id = Guid.NewGuid().ToString(),
                                Type = SecretTypes.SharedSecret,
                                Value = "SampleClient"
                            }
                        },
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
                        ApplicationType = ApplicationTypes.web,
                        RedirectionUrls = "https://sampleclient/callback",
                        UpdateDateTime = DateTime.UtcNow,
                        CreateDateTime = DateTime.UtcNow
                    },
                    // Simple Identity server test client
                    new EF.Models.Client
                    {
                        ClientId = "SimpleIdServerClient",
                        ClientName = "Simple Identity Server Client",
                        ClientSecrets = new List<ClientSecret>
                        {
                            new ClientSecret
                            {
                                Id = Guid.NewGuid().ToString(),
                                Type = SecretTypes.SharedSecret,
                                Value = "SimpleIdServerClient"
                            }
                        },
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
                            },
                            new ClientScope
                            {
                                ScopeName = "address"
                            }
                        },
                        GrantTypes = "1",
                        ResponseTypes = "0,1,2",
                        IdTokenSignedResponseAlg = "RS256",
                        ApplicationType = ApplicationTypes.web,
                        RedirectionUrls = "http://localhost:60000/User/Callback",
                        UpdateDateTime = DateTime.UtcNow,
                        CreateDateTime = DateTime.UtcNow
                    },
                    new EF.Models.Client // Resource manager API.
                    {
                        ClientId = "ResourceManagerApi",
                        ClientName = "ResourceManagerApi",
                        ClientSecrets = new List<ClientSecret>
                        {
                            new ClientSecret
                            {
                                Id = Guid.NewGuid().ToString(),
                                Type = SecretTypes.SharedSecret,
                                Value = "ResourceManagerApi"
                            }
                        },
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
                                ScopeName = "display_configuration"
                            },
                            new ClientScope
                            {
                                ScopeName = "manage_configuration"
                            }
                        },
                        GrantTypes = "1", // Client credentials.
                        ResponseTypes = "2", // id_token.
                        IdTokenSignedResponseAlg = "RS256",
                        ApplicationType = ApplicationTypes.native,
                        UpdateDateTime = DateTime.UtcNow,
                        CreateDateTime = DateTime.UtcNow
                    }
                });
            }
        }
    }
}
