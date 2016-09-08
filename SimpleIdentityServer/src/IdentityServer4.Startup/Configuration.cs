using IdentityServer4.Models;
using System.Collections.Generic;

namespace IdentityServer4.Startup
{
    internal static class Configuration
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
                
            };
        }
    }
}
