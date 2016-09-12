using IdentityServer4.Models;
using IdentityServer4.Quickstart;
using IdentityServer4.Startup.Services;
using System.Collections.Generic;
using System.Security.Claims;

namespace IdentityServer4.Startup
{
    internal static class Config
    {
        /// <summary>
        /// Get scopes
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<Scope> GetScopes()
        {
            return new List<Scope>
            {
                StandardScopes.OpenId,
                StandardScopes.Profile,
                StandardScopes.Email,
                StandardScopes.Address,
                StandardScopes.Phone,
                new Scope
                {
                    Name = "role",
                    DisplayName = "User roles",
                    Type = ScopeType.Identity,
                    Emphasize = true,
                    Claims = new List<ScopeClaim>()
                    {
                      new ScopeClaim("role", false)
                    }
                },
                new Scope
                {
                    Name = "uma_protection",
                    DisplayName = "UMA protection",
                    Description = "Access to UMA permission, resource set & token introspection endpoints",
                    Type = ScopeType.Resource
                },
                new Scope
                {
                    Name = "uma_authorization",
                    Description = "Access to the UMA authorization endpoint",
                    DisplayName = "UMA authorization",
                    Type = ScopeType.Resource
                },
                new Scope
                {
                    Name = "manage_configuration",
                    Description = "Manage configuration",
                    DisplayName = "Manage configuration",
                    Type = ScopeType.Resource
                },
                new Scope
                {
                    Name = "display_configuration",
                    Description = "Display configuration",
                    DisplayName = "Display configuration",
                    Type = ScopeType.Resource
                },
                new Scope
                {
                    Name = "website_api",
                    Description = "Access to the website API",
                    DisplayName = "Website API",
                    Type = ScopeType.Resource
                }
            };
        }

        /// <summary>
        /// Get clients
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<Client> GetClients()
        {
            return new List<Client>
            {
                new Client
                {
                    ClientId = "mvc",
                    ClientName = "MVC Client",
                    AllowedGrantTypes = GrantTypes.HybridAndClientCredentials,

                    ClientSecrets = new List<Secret>
                    {
                        new Secret("secret".Sha256())
                    },

                    RedirectUris = new List<string>
                    {
                        "http://localhost:4001/signin-oidc"
                    },
                    PostLogoutRedirectUris = new List<string>
                    {
                        "http://localhost:4001"
                    },

                    AllowedScopes = new List<string>
                    {
                        StandardScopes.OpenId.Name,
                        StandardScopes.Profile.Name,
                        StandardScopes.OfflineAccess.Name,
                        "api1"
                    }
                }
            };
        }

        /// <summary>
        /// Get resource owners
        /// </summary>
        /// <returns></returns>
        public static List<DbUser> GetUsers()
        {
            return new List<DbUser>
            {
                new DbUser
                {
                    Subject = "1",
                    Username = "alice",
                    Password = "password",
                    IsLocalAccount = true,
                    Claims = new List<DbClaim>
                    {
                        new DbClaim("name", "Alice"),
                        new DbClaim("website", "https://alice.com")
                    }
                },
                new DbUser
                {
                    Subject = "2",
                    Username = "bob",
                    Password = "password",
                    IsLocalAccount = true,
                    Claims = new List<DbClaim>
                    {
                        new DbClaim("name", "Bob"),
                        new DbClaim("website", "https://bob.com")
                    }
                }
            };
        }
    }
}
