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
                                Core.Constants.StandardResourceOwnerClaimNames.Name,
                                Core.Constants.StandardResourceOwnerClaimNames.FamilyName,
                                Core.Constants.StandardResourceOwnerClaimNames.GivenName,
                                Core.Constants.StandardResourceOwnerClaimNames.MiddleName,
                                Core.Constants.StandardResourceOwnerClaimNames.NickName,
                                Core.Constants.StandardResourceOwnerClaimNames.PreferredUserName,
                                Core.Constants.StandardResourceOwnerClaimNames.Profile,
                                Core.Constants.StandardResourceOwnerClaimNames.Picture,
                                Core.Constants.StandardResourceOwnerClaimNames.WebSite,
                                Core.Constants.StandardResourceOwnerClaimNames.Gender,
                                Core.Constants.StandardResourceOwnerClaimNames.BirthDate,
                                Core.Constants.StandardResourceOwnerClaimNames.ZoneInfo,
                                Core.Constants.StandardResourceOwnerClaimNames.Locale,
                                Core.Constants.StandardResourceOwnerClaimNames.UpdatedAt
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
                    IdTokenSignedTResponseAlg = "none"
                }
            };
        }
    }
}