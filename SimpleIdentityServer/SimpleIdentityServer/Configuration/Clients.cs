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
                    IdTokenSignedTResponseAlg = "none",
                    RedirectionUrls = new List<RedirectionUrl>
                    {
                        new RedirectionUrl
                        {
                            Url = "https://op.certification.openid.net:60360/authz_cb"
                        },
                        new RedirectionUrl
                        {
                            Url = "http://localhost"
                        }
                    }
                }
            };
        }
    }
}