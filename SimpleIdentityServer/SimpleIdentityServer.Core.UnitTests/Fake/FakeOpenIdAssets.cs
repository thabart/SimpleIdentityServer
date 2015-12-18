using SimpleIdentityServer.Core.Extensions;
using SimpleIdentityServer.DataAccess.Fake.Models;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace SimpleIdentityServer.Core.UnitTests.Fake
{
    public static class FakeOpenIdAssets
    {
        /// <summary>
        /// Get a list of fake clients
        /// </summary>
        /// <returns></returns>
        public static List<Client> GetClients()
        {
            return new List<Client>
            {
                new Client
                {
                    ClientId = "MyBlog",
                    DisplayName = "My blog",
                    ClientSecret = "MyBlog",
                    TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.client_secret_basic,
                    AllowedScopes = new List<Scope>
                    {
                        // PROTECTED API SCOPES
                        new Scope
                        {
                            Name = "BlogApi"
                        },
                        new Scope
                        {
                            Name = "BlogApi:AddArticle"
                        },
                        new Scope
                        {
                            Name = "openid",
                            IsExposed = true,
                            IsInternal = true,
                            Description = "openid",
                            Type = ScopeType.ProtectedApi
                        },
                        // RO SCOPES
                        new Scope
                        {
                            Name = "profile",
                            IsExposed = true,
                            IsInternal = true,
                            Description = "Access to the profile",
                            Claims = new List<string>
                            {
                                Core.Jwt.Constants.StandardResourceOwnerClaimNames.Name,
                                Core.Jwt.Constants.StandardResourceOwnerClaimNames.FamilyName,
                                Core.Jwt.Constants.StandardResourceOwnerClaimNames.GivenName,
                                Core.Jwt.Constants.StandardResourceOwnerClaimNames.MiddleName,
                                Core.Jwt.Constants.StandardResourceOwnerClaimNames.NickName,
                                Core.Jwt.Constants.StandardResourceOwnerClaimNames.PreferredUserName,
                                Core.Jwt.Constants.StandardResourceOwnerClaimNames.Profile,
                                Core.Jwt.Constants.StandardResourceOwnerClaimNames.Picture,
                                Core.Jwt.Constants.StandardResourceOwnerClaimNames.WebSite,
                                Core.Jwt.Constants.StandardResourceOwnerClaimNames.Gender,
                                Core.Jwt.Constants.StandardResourceOwnerClaimNames.BirthDate,
                                Core.Jwt.Constants.StandardResourceOwnerClaimNames.ZoneInfo,
                                Core.Jwt.Constants.StandardResourceOwnerClaimNames.Locale,
                                Core.Jwt.Constants.StandardResourceOwnerClaimNames.UpdatedAt
                            },
                            Type = ScopeType.ResourceOwner
                        },
                        new Scope
                        {
                            Name = "email",
                            IsExposed = true,
                            IsInternal = true,
                            IsDisplayedInConsent = true,
                            Description = "Access to the email",
                            Claims = new List<string>
                            {
                                Core.Jwt.Constants.StandardResourceOwnerClaimNames.Email,
                                Core.Jwt.Constants.StandardResourceOwnerClaimNames.EmailVerified
                            },
                            Type = ScopeType.ResourceOwner
                        },
                        new Scope
                        {
                            Name = "address",
                            IsExposed = true,
                            IsInternal = true,
                            IsDisplayedInConsent = true,
                            Description = "Access to the address",
                            Claims = new List<string>
                            {
                                Core.Jwt.Constants.StandardResourceOwnerClaimNames.Address
                            },
                            Type = ScopeType.ResourceOwner
                        },
                        new Scope
                        {
                            Name = "phone",
                            IsExposed = true,
                            IsInternal = true,
                            IsDisplayedInConsent = true,
                            Description = "Access to the phone",
                            Claims = new List<string>
                            {
                                Core.Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumber,
                                Core.Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumberVerified
                            },
                            Type = ScopeType.ResourceOwner
                        }
                    },
                    GrantTypes = new List<GrantType>
                    {
                        GrantType.@implicit,
                        GrantType.authorization_code
                    },
                    ResponseTypes = new List<ResponseType>
                    {
                        ResponseType.token,
                        ResponseType.code,
                        ResponseType.id_token
                    },
                    IdTokenSignedTResponseAlg = "RS256",
                    // IdTokenEncryptedResponseAlg = "RSA1_5",
                    // IdTokenEncryptedResponseEnc = "A128CBC-HS256",
                    RedirectionUrls = new List<RedirectionUrl>
                    {
                        new RedirectionUrl
                        {
                            Url = "https://op.certification.openid.net:60360/authz_cb"
                        },
                        new RedirectionUrl
                        {
                            Url = "http://localhost"
                        },
                        new RedirectionUrl
                        {
                            Url = "https://op.certification.openid.net:60186/authz_cb"
                        }
                    },
                }
            };
        }

        /// <summary>
        /// Get a list of scopes
        /// </summary>
        /// <returns></returns>
        public static List<Scope> GetScopes()
        {
            return new List<Scope>
            {
                new Scope
                {
                    Name = "BlogApi",
                    Description = "Access to the blog API",
                    IsInternal = false,
                    IsDisplayedInConsent = true
                },
                new Scope
                {
                    Name = "BlogApi:AddArticle",
                    Description = "Access to the add article operation",
                    IsInternal = false,
                    IsDisplayedInConsent = true
                },
                new Scope
                {
                    Name = "openid",
                    IsExposed = true,
                    IsInternal = true,
                    IsDisplayedInConsent = false,
                    Description = "openid",
                    Type = ScopeType.ProtectedApi
                },
                new Scope
                {
                    Name = "profile",
                    IsExposed = true,
                    IsInternal = true,
                    Description = "Access to the profile",
                    Claims = new List<string>
                    {
                        Core.Jwt.Constants.StandardResourceOwnerClaimNames.Name,
                        Core.Jwt.Constants.StandardResourceOwnerClaimNames.FamilyName,
                        Core.Jwt.Constants.StandardResourceOwnerClaimNames.GivenName,
                        Core.Jwt.Constants.StandardResourceOwnerClaimNames.MiddleName,
                        Core.Jwt.Constants.StandardResourceOwnerClaimNames.NickName,
                        Core.Jwt.Constants.StandardResourceOwnerClaimNames.PreferredUserName,
                        Core.Jwt.Constants.StandardResourceOwnerClaimNames.Profile,
                        Core.Jwt.Constants.StandardResourceOwnerClaimNames.Picture,
                        Core.Jwt.Constants.StandardResourceOwnerClaimNames.WebSite,
                        Core.Jwt.Constants.StandardResourceOwnerClaimNames.Gender,
                        Core.Jwt.Constants.StandardResourceOwnerClaimNames.BirthDate,
                        Core.Jwt.Constants.StandardResourceOwnerClaimNames.ZoneInfo,
                        Core.Jwt.Constants.StandardResourceOwnerClaimNames.Locale,
                        Core.Jwt.Constants.StandardResourceOwnerClaimNames.UpdatedAt
                    },
                    Type = ScopeType.ResourceOwner,
                    IsDisplayedInConsent = true
                },
                new Scope
                {
                    Name = "email",
                    IsExposed = true,
                    IsInternal = true,
                    IsDisplayedInConsent = true,
                    Description = "Access to the email",
                    Claims = new List<string>
                    {
                        Core.Jwt.Constants.StandardResourceOwnerClaimNames.Email,
                        Core.Jwt.Constants.StandardResourceOwnerClaimNames.EmailVerified
                    },
                    Type = ScopeType.ResourceOwner
                },
                new Scope
                {
                    Name = "address",
                    IsExposed = true,
                    IsInternal = true,
                    IsDisplayedInConsent = true,
                    Description = "Access to the address",
                    Claims = new List<string>
                    {
                        Core.Jwt.Constants.StandardResourceOwnerClaimNames.Address
                    },
                    Type = ScopeType.ResourceOwner
                },
                new Scope
                {
                    Name = "phone",
                    IsExposed = true,
                    IsInternal = true,
                    IsDisplayedInConsent = true,
                    Description = "Access to the phone",
                    Claims = new List<string>
                    {
                        Core.Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumber,
                        Core.Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumberVerified
                    },
                    Type = ScopeType.ResourceOwner
                }
            };
        }

        /// <summary>
        /// Get a list of fake resource owners.
        /// </summary>
        /// <returns></returns>
        public static List<ResourceOwner> GetResourceOwners()
        {
            return new List<ResourceOwner>
            {
                new ResourceOwner
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
                    UpdatedAt = DateTime.UtcNow.ConvertToUnixTimestamp(),
                    WebSite = "https://github.com/thabart",
                    ZoneInfo = "Europe/Paris",
                    Password = "5E884898DA28047151D0E56F8DC6292773603D0D6AABBDD62A11EF721D1542D8"
                }
            };
        }

        public static List<Consent> GetConsents()
        {
            return new List<Consent>();
        }

        public static List<JsonWebKey> GetJsonWebKeys()
        {
            var serializedRsa = string.Empty;
            using (var provider = new RSACryptoServiceProvider())
            {
                serializedRsa = provider.ToXmlString(true);
            }

            return new List<JsonWebKey>
            {
                new JsonWebKey
                {
                    Alg = AllAlg.RS256,
                    KeyOps = new []
                    {
                        KeyOperations.Sign,
                        KeyOperations.Verify
                    },
                    Kid = "a3rMUgMFv9tPclLa6yF3zAkfquE",
                    Kty = KeyType.RSA,
                    Use = Use.Sig,
                    SerializedKey = serializedRsa,
                },
                new JsonWebKey
                {
                    Alg = AllAlg.RSA1_5,
                    KeyOps = new []
                    {
                        KeyOperations.Encrypt,
                        KeyOperations.Decrypt
                    },
                    Kid = "3",
                    Kty = KeyType.RSA,
                    Use = Use.Enc,
                    SerializedKey = serializedRsa,
                }
            };
        }
    }
}
