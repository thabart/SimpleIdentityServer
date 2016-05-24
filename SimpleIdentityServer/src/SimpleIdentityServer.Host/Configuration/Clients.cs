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
    public static class Clients
    {
        public static List<Client> Get()
        {
            return new List<Client>
            {
                new Client
                {
                    ClientId = "MyBlog",
                    ClientName = "My blog",
                    ClientSecret = "MyBlog",
                    TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.client_secret_post,
                    LogoUri = "http://img.over-blog-kiwi.com/1/47/73/14/20150513/ob_06dc4f_chiot-shiba-inu-a-vendre-prix-2015.jpg",
                    PolicyUri = "http://openid.net",
                    TosUri = "http://openid.net",
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
                            Name = "Values:Get"
                        },
                        new Scope
                        {
                            Name = "openid",
                            IsExposed = true,
                            IsOpenIdScope = true,
                            Description = "openid",
                            Type = ScopeType.ProtectedApi
                        },
                        // RO SCOPES
                        new Scope
                        {
                            Name = "profile",
                            IsExposed = true,
                            IsOpenIdScope = true,
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
                            IsOpenIdScope = true,
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
                            IsOpenIdScope = true,
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
                            IsOpenIdScope = true,
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
                    IdTokenSignedResponseAlg = "RS256",
                    IdTokenEncryptedResponseAlg = "RSA1_5",
                    IdTokenEncryptedResponseEnc = "A128CBC-HS256",
                    RedirectionUrls = new List<string>
                    {
                        "https://op.certification.openid.net:60360/authz_cb",
                        "http://localhost",
                        "https://op.certification.openid.net:60186/authz_cb",
                        "https://op.certification.openid.net:60428/authz_cb"
                    }
                },
                new Client
                {
                    ClientId = "MyBlogClientSecretPost",
                    ClientName = "My blog",
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
                            IsOpenIdScope = true,
                            Description = "openid",
                            Type = ScopeType.ProtectedApi
                        },
                        // RO SCOPES
                        new Scope
                        {
                            Name = "profile",
                            IsExposed = true,
                            IsOpenIdScope = true,
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
                            IsOpenIdScope = true,
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
                            IsOpenIdScope = true,
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
                            IsOpenIdScope = true,
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
                    IdTokenSignedResponseAlg = "RS256",
                    // IdTokenEncryptedResponseAlg = "RSA1_5",
                    // IdTokenEncryptedResponseEnc = "A128CBC-HS256",
                    RedirectionUrls = new List<string>
                    {
                        "https://op.certification.openid.net:60360/authz_cb",
                        "http://localhost",
                        "https://op.certification.openid.net:60186/authz_cb",
                        "https://op.certification.openid.net:60428/authz_cb"
                    }
                }
            };
        }
    }
}