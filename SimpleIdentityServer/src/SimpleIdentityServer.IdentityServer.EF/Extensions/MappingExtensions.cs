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

using IdentityServer4.Models;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.IdentityServer.EF.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleIdentityServer.IdentityServer.EF
{
    internal static class MappingExtensions
    {
        private class GrantingMethod
        {
            public List<ResponseType> ResponseTypes { get; set; }

            public List<Core.Models.GrantType> GrantTypes { get; set; }

            public override bool Equals(object obj)
            {
                var o = obj as GrantingMethod;
                if (o == null)
                {
                    return false;
                }

                return o.GrantTypes.Count() == GrantTypes.Count() && o.GrantTypes.All(g => GrantTypes.Contains(g))
                    && o.ResponseTypes.Count() == ResponseTypes.Count() && o.ResponseTypes.All(r => ResponseTypes.Contains(r));
            }

            public bool Equals(GrantingMethod obj)
            {
                if (obj == null)
                {
                    return false;
                }

                return Equals((object)obj);
            }

            public override int GetHashCode()
            {
                return ResponseTypes.GetHashCode() ^ GrantTypes.GetHashCode();
            }
        }

        #region Fields

        private static Dictionary<IEnumerable<string>, GrantingMethod> _mappingIdServerGrantTypesToGrantingMethods = new Dictionary<IEnumerable<string>, GrantingMethod>
        {
            {
                IdentityServer4.Models.GrantTypes.Implicit,
                new GrantingMethod
                {
                    ResponseTypes = new List<ResponseType>
                    {
                        ResponseType.id_token,
                        ResponseType.token
                    },
                    GrantTypes = new List<Core.Models.GrantType>
                    {
                        Core.Models.GrantType.@implicit
                    }
                }
            },
            {
                IdentityServer4.Models.GrantTypes.ImplicitAndClientCredentials,
                new GrantingMethod
                {
                    ResponseTypes = new List<ResponseType>
                    {
                        ResponseType.id_token,
                        ResponseType.token
                    },
                    GrantTypes = new List<Core.Models.GrantType>
                    {
                        Core.Models.GrantType.@implicit,
                        Core.Models.GrantType.client_credentials
                    }
                }
            },
            {
                IdentityServer4.Models.GrantTypes.Code,
                new GrantingMethod
                {
                    ResponseTypes = new List<ResponseType>
                    {
                        ResponseType.code
                    },
                    GrantTypes = new List<Core.Models.GrantType>
                    {
                        Core.Models.GrantType.authorization_code
                    }
                }
            },
            {
                IdentityServer4.Models.GrantTypes.CodeAndClientCredentials,
                new GrantingMethod
                {
                    ResponseTypes = new List<ResponseType>
                    {
                        ResponseType.code,
                        ResponseType.token
                    },
                    GrantTypes = new List<Core.Models.GrantType>
                    {
                        Core.Models.GrantType.authorization_code,
                        Core.Models.GrantType.client_credentials
                    }
                }
            },
            {
                IdentityServer4.Models.GrantTypes.Hybrid,
                new GrantingMethod
                {
                    ResponseTypes = new List<ResponseType>
                    {
                        ResponseType.code,
                        ResponseType.id_token,
                        ResponseType.token
                    },
                    GrantTypes = new List<Core.Models.GrantType>
                    {
                        Core.Models.GrantType.@implicit,
                        Core.Models.GrantType.authorization_code,
                        Core.Models.GrantType.refresh_token
                    }
                }
            },
            {
                IdentityServer4.Models.GrantTypes.HybridAndClientCredentials,
                new GrantingMethod
                {
                    ResponseTypes = new List<ResponseType>
                    {
                        ResponseType.code,
                        ResponseType.id_token,
                        ResponseType.token
                    },
                    GrantTypes = new List<Core.Models.GrantType>
                    {
                        Core.Models.GrantType.@implicit,
                        Core.Models.GrantType.authorization_code,
                        Core.Models.GrantType.client_credentials
                    }
                }
            },
            {
                IdentityServer4.Models.GrantTypes.ClientCredentials,
                new GrantingMethod
                {
                    ResponseTypes = new List<ResponseType>
                    {
                        ResponseType.token
                    },
                    GrantTypes = new List<Core.Models.GrantType>
                    {
                        Core.Models.GrantType.client_credentials
                    }
                }
            },
            {
                IdentityServer4.Models.GrantTypes.ResourceOwnerPassword,
                new GrantingMethod
                {
                    ResponseTypes = new List<ResponseType>
                    {
                        ResponseType.token
                    },
                    GrantTypes = new List<Core.Models.GrantType>
                    {
                        Core.Models.GrantType.password,
                        Core.Models.GrantType.refresh_token
                    }
                }
            },
            {
                IdentityServer4.Models.GrantTypes.ResourceOwnerPasswordAndClientCredentials,
                new GrantingMethod
                {
                    ResponseTypes = new List<ResponseType>
                    {
                        ResponseType.token
                    },
                    GrantTypes = new List<Core.Models.GrantType>
                    {
                        Core.Models.GrantType.password,
                        Core.Models.GrantType.client_credentials
                    }
                }
            }
        };

        #endregion

        #region To domain

        public static Core.Models.Scope ToDomain(this IdentityServer4.EntityFramework.Entities.Scope scope)
        {
            var standardScopeNames = IdentityServer4.Models.StandardScopes.All.Select(s => s.Name);
            var record = new Core.Models.Scope
            {
                Name = scope.Name,
                Description = scope.Description,
                IsExposed = scope.ShowInDiscoveryDocument,
                IsDisplayedInConsent = true
            };
            record.IsOpenIdScope = standardScopeNames.Contains(record.Name);
            record.Type = scope.Type == (int)IdentityServer4.Models.ScopeType.Identity ?
                Core.Models.ScopeType.ResourceOwner : Core.Models.ScopeType.ProtectedApi;
            if (scope.Claims != null && scope.Claims.Any())
            {
                record.Claims = scope.Claims.Select(c => c.Name).ToList();
            }

            return record;
        }

        public static ResourceOwner ToDomain(this User user)
        {
            var result = new ResourceOwner
            {
                Id = user.Subject,
                Password = user.Password,
                Name = user.Username,
                IsLocalAccount = user.IsLocalAccount
            };

            if (user.Claims != null && user.Claims.Any())
            {
                result.BirthDate = user.Claims.GetClaim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.BirthDate);
                result.Email = user.Claims.GetClaim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.Email);
                result.FamilyName = user.Claims.GetClaim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.FamilyName);
                result.Gender = user.Claims.GetClaim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.Gender);
                result.GivenName = user.Claims.GetClaim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.GivenName);
                result.Locale = user.Claims.GetClaim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.Locale);
                result.MiddleName = user.Claims.GetClaim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.MiddleName);
                result.NickName = user.Claims.GetClaim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.NickName);
                result.Picture = user.Claims.GetClaim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.Picture);
                result.PhoneNumber = user.Claims.GetClaim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumber);
                result.PreferredUserName = user.Claims.GetClaim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.PreferredUserName);
                result.Profile = user.Claims.GetClaim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.Profile);
                result.WebSite = user.Claims.GetClaim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.WebSite);
                result.ZoneInfo = user.Claims.GetClaim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.ZoneInfo);
                result.BirthDate = user.Claims.GetClaim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.BirthDate);
                result.EmailVerified = user.Claims.GetClaimBool(Core.Jwt.Constants.StandardResourceOwnerClaimNames.EmailVerified);
                result.PhoneNumberVerified = user.Claims.GetClaimBool(Core.Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumberVerified);
                result.Roles = user.Claims.GetClaims(Core.Jwt.Constants.StandardResourceOwnerClaimNames.Role);
            }

            return result;
        }

        public static Core.Models.Scope ToDomain(this IdentityServer4.EntityFramework.Entities.ClientScope scope)
        {
            return new Core.Models.Scope
            {
                Name = scope.Scope
            };
        }

        public static Core.Models.Client ToDomain(this IdentityServer4.EntityFramework.Entities.Client client)
        {
            var result = new Core.Models.Client
            {
                ClientId = client.ClientId,
                ClientSecret =  client.ClientSecrets == null || !client.ClientSecrets.Any() ? string.Empty : client.ClientSecrets.First().Value,
                LogoUri = client.LogoUri,
                ClientName = client.ClientName,
                ClientUri = client.ClientUri,
                IdTokenSignedResponseAlg = "RS256",
                IdTokenEncryptedResponseAlg = null,
                IdTokenEncryptedResponseEnc = null,
                TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.client_secret_basic,
                AllowedScopes = client.AllowedScopes == null || !client.AllowedScopes.Any() ? new List<Core.Models.Scope>() : client.AllowedScopes.Select(s => s.ToDomain()).ToList(),
                RedirectionUrls = client.RedirectUris == null || !client.RedirectUris.Any() ? new List<string>() : client.RedirectUris.Select(s => s.RedirectUri).ToList(),
                ApplicationType = ApplicationTypes.web
            };

            if (client.AllowedGrantTypes != null)
            {
                var grantingMethod = _mappingIdServerGrantTypesToGrantingMethods.FirstOrDefault(m =>
                    client.AllowedGrantTypes.Count() == m.Key.Count() &&
                    m.Key.All(g => client.AllowedGrantTypes.Any(c => c.GrantType == g)));
                if (!grantingMethod.Equals(default(KeyValuePair<IEnumerable<string>, GrantingMethod>)))
                {
                    result.ResponseTypes = grantingMethod.Value.ResponseTypes;
                    result.GrantTypes = grantingMethod.Value.GrantTypes;
                }
            }

            return result;
        }

        #endregion

        #region To Entity

        public static User ToEntity(this ResourceOwner resourceOwner)
        {
            var result = new User
            {
                Subject = resourceOwner.Id,
                Password = resourceOwner.Password,
                Username = resourceOwner.Name,
                IsLocalAccount = resourceOwner.IsLocalAccount,
                Claims = new List<Claim>()
            };

            result.Claims.Add(CreateClaim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.BirthDate, resourceOwner.BirthDate));
            result.Claims.Add(CreateClaim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.Email, resourceOwner.Email));
            result.Claims.Add(CreateClaim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.FamilyName, resourceOwner.FamilyName));
            result.Claims.Add(CreateClaim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.Gender, resourceOwner.Gender));
            result.Claims.Add(CreateClaim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.GivenName, resourceOwner.GivenName));
            result.Claims.Add(CreateClaim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.Locale, resourceOwner.Locale));
            result.Claims.Add(CreateClaim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.MiddleName, resourceOwner.MiddleName));
            result.Claims.Add(CreateClaim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.NickName, resourceOwner.NickName));
            result.Claims.Add(CreateClaim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.Picture, resourceOwner.Picture));
            result.Claims.Add(CreateClaim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumber, resourceOwner.PhoneNumber));
            result.Claims.Add(CreateClaim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.PreferredUserName, resourceOwner.PreferredUserName));
            result.Claims.Add(CreateClaim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.Profile, resourceOwner.Profile));
            result.Claims.Add(CreateClaim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.WebSite, resourceOwner.WebSite));
            result.Claims.Add(CreateClaim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.ZoneInfo, resourceOwner.ZoneInfo));
            result.Claims.Add(CreateClaim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.EmailVerified, resourceOwner.EmailVerified.ToString()));
            result.Claims.Add(CreateClaim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumberVerified, resourceOwner.PhoneNumberVerified.ToString()));
            if (resourceOwner.Roles != null && resourceOwner.Roles.Any())
            {
                foreach(var role in resourceOwner.Roles)
                {
                    result.Claims.Add(CreateClaim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.Role,  role));
                }
            }

            return result;
        }

        public static IdentityServer4.EntityFramework.Entities.Client ToEntity(this Core.Models.Client client)
        {
            var result = new IdentityServer4.EntityFramework.Entities.Client
            {
                ClientId = client.ClientId,
                ClientName = client.ClientName,
                Enabled = true,
                RequireClientSecret = true,
                RequireConsent = true,
                AllowRememberConsent = true,
                LogoutSessionRequired = true,
                IdentityTokenLifetime = 300,
                AccessTokenLifetime = 3600,
                AuthorizationCodeLifetime = 300,
                AbsoluteRefreshTokenLifetime = 2592000,
                SlidingRefreshTokenLifetime = 1296000,
                RefreshTokenUsage = (int)TokenUsage.OneTimeOnly,
                RefreshTokenExpiration = (int)TokenExpiration.Absolute,
                EnableLocalLogin = true,
                PrefixClientClaims = true,
                AllowAccessTokensViaBrowser = true,
                LogoUri = client.LogoUri,
                ClientUri = client.ClientUri,
                AllowedScopes = client.AllowedScopes == null || !client.AllowedScopes.Any() ? new List<IdentityServer4.EntityFramework.Entities.ClientScope>() : client.AllowedScopes.Select(s => new IdentityServer4.EntityFramework.Entities.ClientScope
                {
                    Scope = s.Name
                }).ToList(),
                RedirectUris = client.RedirectionUrls == null || !client.RedirectionUrls.Any() ? new List<IdentityServer4.EntityFramework.Entities.ClientRedirectUri>() : client.RedirectionUrls.Select(r => new IdentityServer4.EntityFramework.Entities.ClientRedirectUri
                {
                    RedirectUri = r
                }).ToList(),
                ClientSecrets = string.IsNullOrWhiteSpace(client.ClientSecret) ? new List<IdentityServer4.EntityFramework.Entities.ClientSecret>() : new List<IdentityServer4.EntityFramework.Entities.ClientSecret>
                {
                    new IdentityServer4.EntityFramework.Entities.ClientSecret
                    {
                        Value = client.ClientSecret,
                        Type = "SharedSecret"
                    }
                }
            };

            var grantingMethod = new GrantingMethod
            {
                GrantTypes = client.GrantTypes,
                ResponseTypes = client.ResponseTypes
            };

            var meth = _mappingIdServerGrantTypesToGrantingMethods.FirstOrDefault(m => m.Value.Equals(grantingMethod));
            if (meth.Equals(default(KeyValuePair<IEnumerable<string>, GrantingMethod>)))
            {
                throw new InvalidOperationException("The grant type is not supported");
            }

            result.AllowedGrantTypes = meth.Key.Select(g => new IdentityServer4.EntityFramework.Entities.ClientGrantType { GrantType = g }).ToList();
            return result;
        }

        #endregion

        #region Private static methods

        private static Claim CreateClaim(string key, string value)
        {
            return new Claim
            {
                Key = key,
                Value = value,
                Id = Guid.NewGuid().ToString()
            };
        }

        private static string GetClaim(this IEnumerable<Claim> claims, string key)
        {
            var claim = claims.FirstOrDefault(c => c.Key == key);
            if (claim == null)
            {
                return string.Empty;
            }

            return claim.Value;
        }

        private static bool GetClaimBool(this IEnumerable<Claim> claims, string key)
        {
            var claim = claims.FirstOrDefault(c => c.Key == key);
            if (claim == null)
            {
                return false;
            }

            bool result = false;
            if (!bool.TryParse(claim.Value, out result))
            {
                return result;
            }

            return result;
        }

        private static List<string> GetClaims(this IEnumerable<Claim> claims, string key)
        {
            return claims.Where(c => c.Key == key).Select(c => c.Value).ToList();
        }

        #endregion
    }
}
