using System.Collections.Generic;
using SimpleIdentityServer.DataAccess.Fake.Models;

namespace SimpleIdentityServer.Api.Configuration
{
    public static class Clients
    {
        public static List<Client> Get()
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
                        },
                        new RedirectionUrl
                        {
                            Url = "https://op.certification.openid.net:60428/authz_cb"
                        }
                    },
                },
                new Client
                {
                    ClientId = "MyBlogClientSecretPost",
                    DisplayName = "My blog",
                    ClientSecret = "MyBlogClientSecretPost",
                    TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.client_secret_post,
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
                        },
                        new RedirectionUrl
                        {
                            Url = "https://op.certification.openid.net:60428/authz_cb"
                        }
                    },
                }
            };
        }
    }
}