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
using SimpleIdentityServer.DataAccess.SqlServer;
using SimpleIdentityServer.DataAccess.SqlServer.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;

namespace SimpleIdentityServer.Host.Tests.Extensions
{
    public static class SimpleIdentityServerContextExtensions
    {
        public static void EnsureSeedData(this SimpleIdentityServerContext context, SharedContext sharedCtx)
        {
            InsertClaims(context);
            InsertScopes(context);
            InsertResourceOwners(context);
            InsertJsonWebKeys(context);
            InsertClients(context, sharedCtx);
            InsertConsents(context);
            InsertConsentScopes(context);
            try
            {
                context.SaveChanges();
            }
            catch
            {
                Trace.WriteLine("items already exists");
            }
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
                        Name = "api1",
                        IsExposed = false,
                        IsOpenIdScope = false,
                        IsDisplayedInConsent = true,
                        Description = "Access to your api1",
                        Type = ScopeType.ProtectedApi
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
                        Password = "password",
                        IsLocalAccount = true
                    }
                });
            }
        }

        private static void InsertConsents(SimpleIdentityServerContext context)
        {
            if (!context.Consents.Any())
            {
                var consents = new List<Consent>()
                {
                    {
                        new Consent
                        {
                            Id = 1,
                            ClientId = "implicit_client",
                            ResourceOwnerId = "administrator"
                        }
                    }
                };
                context.Consents.AddRange(consents);
            }
        }

        private static void InsertConsentScopes(SimpleIdentityServerContext context)
        {
            if (!context.ConsentScopes.Any())
            {
                var consentScopes = new List<ConsentScope>
                {
                    new ConsentScope
                    {
                        ConsentId = 1,
                        ScopeName = "api1"
                    },
                    new ConsentScope
                    {
                        ConsentId = 1,
                        ScopeName = "openid"
                    }
                };
                context.ConsentScopes.AddRange(consentScopes);
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

        private static void InsertClients(SimpleIdentityServerContext context, SharedContext sharedCtx)
        {
            if (!context.Clients.Any())
            {
                context.Clients.AddRange(new[]
                {
                    new DataAccess.SqlServer.Models.Client
                    {
                        ClientId = "client",
                        ClientName = "client",
                        ClientSecret = "client",
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
                                ScopeName = "scim"
                            }
                        },
                        GrantTypes = "4",
                        ResponseTypes = "0,1,2",
                        IdTokenSignedResponseAlg = "RS256",
                        ApplicationType = ApplicationTypes.web,
                        RedirectionUrls = "https://localhost:4200/callback"
                    },
                    new DataAccess.SqlServer.Models.Client
                    {
                        ClientId = "basic_client",
                        ClientName = "basic_client",
                        ClientSecret = "basic_client",
                        TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.client_secret_basic,
                        LogoUri = "http://img.over-blog-kiwi.com/1/47/73/14/20150513/ob_06dc4f_chiot-shiba-inu-a-vendre-prix-2015.jpg",
                        PolicyUri = "http://openid.net",
                        TosUri = "http://openid.net",
                        ClientScopes = new List<ClientScope>
                        {
                            new ClientScope
                            {
                                ScopeName = "api1"
                            }
                        },
                        GrantTypes = "3",
                        ResponseTypes = "1",
                        IdTokenSignedResponseAlg = "RS256",
                        ApplicationType = ApplicationTypes.web,
                        RedirectionUrls = "https://localhost:4200/callback"
                    },
                    new DataAccess.SqlServer.Models.Client
                    {
                        ClientId = "post_client",
                        ClientName = "post_client",
                        ClientSecret = "post_client",
                        TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.client_secret_post,
                        LogoUri = "http://img.over-blog-kiwi.com/1/47/73/14/20150513/ob_06dc4f_chiot-shiba-inu-a-vendre-prix-2015.jpg",
                        PolicyUri = "http://openid.net",
                        TosUri = "http://openid.net",
                        ClientScopes = new List<ClientScope>
                        {
                            new ClientScope
                            {
                                ScopeName = "api1"
                            }
                        },
                        GrantTypes = "3",
                        ResponseTypes = "1",
                        IdTokenSignedResponseAlg = "RS256",
                        ApplicationType = ApplicationTypes.web,
                        RedirectionUrls = "https://localhost:4200/callback"
                    },
                    new DataAccess.SqlServer.Models.Client
                    {
                        ClientId = "jwt_client",
                        ClientName = "jwt_client",
                        ClientSecret = "jwt_client",
                        TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.client_secret_jwt,
                        LogoUri = "http://img.over-blog-kiwi.com/1/47/73/14/20150513/ob_06dc4f_chiot-shiba-inu-a-vendre-prix-2015.jpg",
                        PolicyUri = "http://openid.net",
                        TosUri = "http://openid.net",
                        ClientScopes = new List<ClientScope>
                        {
                            new ClientScope
                            {
                                ScopeName = "api1"
                            }
                        },
                        GrantTypes = "3",
                        ResponseTypes = "1",
                        IdTokenSignedResponseAlg = "RS256",
                        ApplicationType = ApplicationTypes.web,
                        RedirectionUrls = "https://localhost:4200/callback",
                        JsonWebKeys = new List<JsonWebKey>
                        {
                            sharedCtx.ModelSignatureKey,
                            sharedCtx.ModelEncryptionKey
                        }
                    },
                    new DataAccess.SqlServer.Models.Client
                    {
                        ClientId = "private_key_client",
                        ClientName = "private_key_client",
                        ClientSecret = "private_key_client",
                        TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.private_key_jwt,
                        LogoUri = "http://img.over-blog-kiwi.com/1/47/73/14/20150513/ob_06dc4f_chiot-shiba-inu-a-vendre-prix-2015.jpg",
                        PolicyUri = "http://openid.net",
                        TosUri = "http://openid.net",
                        ClientScopes = new List<ClientScope>
                        {
                            new ClientScope
                            {
                                ScopeName = "api1"
                            }
                        },
                        GrantTypes = "3",
                        ResponseTypes = "1",
                        IdTokenSignedResponseAlg = "RS256",
                        ApplicationType = ApplicationTypes.web,
                        RedirectionUrls = "https://localhost:4200/callback",
                        JwksUri = "http://localhost:5000/jwks_client"
                    },
                    new DataAccess.SqlServer.Models.Client
                    {
                        ClientId = "implicit_client",
                        ClientName = "implicit_client",
                        ClientSecret = "implicit_client",
                        TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.client_secret_post,
                        LogoUri = "http://img.over-blog-kiwi.com/1/47/73/14/20150513/ob_06dc4f_chiot-shiba-inu-a-vendre-prix-2015.jpg",
                        PolicyUri = "http://openid.net",
                        TosUri = "http://openid.net",
                        ClientScopes = new List<ClientScope>
                        {
                            new ClientScope
                            {
                                ScopeName = "api1"
                            },
                            new ClientScope
                            {
                                ScopeName = "openid"
                            }
                        },
                        GrantTypes = "0,1",
                        ResponseTypes = "0,1,2",
                        IdTokenSignedResponseAlg = "RS256",
                        ApplicationType = ApplicationTypes.web,
                        RedirectionUrls = "http://localhost:5000/callback"
                    }
                });
            }
        }
    }
}
