using IdentityServer4.Models;
using IdentityServer4.Startup.Services;
using SimpleIdentityServer.Core.Extensions;
using System;
using System.Collections.Generic;

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
                    Type = ScopeType.Resource,
                    AllowUnrestrictedIntrospection = true,
                    ScopeSecrets = new List<Secret>
                    {
                        new Secret("manage_configuration".Sha256())
                    }
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
                    Type = ScopeType.Resource,
                    AllowUnrestrictedIntrospection = true,
                    ScopeSecrets = new List<Secret>
                    {
                        new Secret("website_api".Sha256())
                    }
                },
                new Scope
                {
                    Name = "openid_manager",
                    Description = "Access to the OpenId Manager",
                    DisplayName = "OpenId Manager",
                    Type = ScopeType.Resource,
                    AllowUnrestrictedIntrospection = true,
                    ScopeSecrets = new List<Secret>
                    {
                        new Secret("openid_manager".Sha256())
                    }
                },
                new Scope
                {
                    Name = "uma",
                    Description = "UMA",
                    DisplayName = "uma",
                    Type = ScopeType.Resource,
                    AllowUnrestrictedIntrospection = true,
                    ScopeSecrets = new List<Secret>
                    {
                        new Secret("uma".Sha256())
                    }
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
                    ClientId = "website",
                    ClientName = "WebSite",
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                    ClientSecrets = new List<Secret>
                    {
                        new Secret("website".Sha256())
                    },
                    AllowedScopes = new List<string>
                    {
                        SimpleIdentityServer.Core.Constants.StandardScopes.OpenId.Name,
                        SimpleIdentityServer.Core.Constants.StandardScopes.ProfileScope.Name,
                        "role",
                        "website_api"
                    }
                },
                new Client
                {
                    ClientId = "ManagerWebSiteApi",
                    ClientName = "Manager website API",
                    ClientSecrets = new List<Secret>
                    {
                        new Secret("ManagerWebSiteApi".Sha256())
                    },
                    LogoUri = "http://img.over-blog-kiwi.com/1/47/73/14/20150513/ob_06dc4f_chiot-shiba-inu-a-vendre-prix-2015.jpg",
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    AllowedScopes = new List<string>
                    {
                        "openid",
                        "uma_protection",
                        "uma_authorization",
                        "openid_manager",
                        "manage_configuration",
                        "display_configuration",
                        "uma"
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
                    Username = "administrator",
                    Password = "password",
                    IsLocalAccount = true,
                    Enabled = true,
                    Claims = new List<DbClaim>
                    {
                        new DbClaim(SimpleIdentityServer.Core.Jwt.Constants.StandardResourceOwnerClaimNames.BirthDate, "1989-10-07"),
                        new DbClaim(SimpleIdentityServer.Core.Jwt.Constants.StandardResourceOwnerClaimNames.Email, "habarthierry@hotmail.fr"),
                        new DbClaim(SimpleIdentityServer.Core.Jwt.Constants.StandardResourceOwnerClaimNames.EmailVerified, "true"),
                        new DbClaim(SimpleIdentityServer.Core.Jwt.Constants.StandardResourceOwnerClaimNames.Gender, "M"),
                        new DbClaim(SimpleIdentityServer.Core.Jwt.Constants.StandardResourceOwnerClaimNames.GivenName, "Habart Thierry"),
                        new DbClaim(SimpleIdentityServer.Core.Jwt.Constants.StandardResourceOwnerClaimNames.Locale, "fr-FR"),
                        new DbClaim(SimpleIdentityServer.Core.Jwt.Constants.StandardResourceOwnerClaimNames.MiddleName, "Thierry"),
                        new DbClaim(SimpleIdentityServer.Core.Jwt.Constants.StandardResourceOwnerClaimNames.NickName, "Titi"),
                        new DbClaim(SimpleIdentityServer.Core.Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumber, "00"),
                        new DbClaim(SimpleIdentityServer.Core.Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumberVerified, "false"),
                        new DbClaim(SimpleIdentityServer.Core.Jwt.Constants.StandardResourceOwnerClaimNames.Picture, "https://upload.wikimedia.org/wikipedia/commons/thumb/5/58/Shiba_inu_taiki.jpg/220px-Shiba_inu_taiki.jpg"),
                        new DbClaim(SimpleIdentityServer.Core.Jwt.Constants.StandardResourceOwnerClaimNames.PreferredUserName, "Thierry"),
                        new DbClaim(SimpleIdentityServer.Core.Jwt.Constants.StandardResourceOwnerClaimNames.Profile, "http://localhost/profile"),
                        new DbClaim(SimpleIdentityServer.Core.Jwt.Constants.StandardResourceOwnerClaimNames.UpdatedAt, DateTime.Now.ConvertToUnixTimestamp().ToString()),
                        new DbClaim(SimpleIdentityServer.Core.Jwt.Constants.StandardResourceOwnerClaimNames.WebSite, "https://github.com/thabart"),
                        new DbClaim(SimpleIdentityServer.Core.Jwt.Constants.StandardResourceOwnerClaimNames.ZoneInfo, "Europe/Paris"),
                        new DbClaim(SimpleIdentityServer.Core.Jwt.Constants.StandardResourceOwnerClaimNames.Role, "administrator")
                    }
                }
            };
        }
    }
}
