using SimpleIdentityServer.Core.Common.Models;
using SimpleIdentityServer.Core.Common.Parameters;
using SimpleIdentityServer.Core.Common.Repositories;
using SimpleIdentityServer.Core.Common.Results;
using SimpleIdentityServer.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Core.Repositories
{
    internal sealed class DefaultScopeRepository : IScopeRepository
    {
        public ICollection<Scope> _scopes;

        private List<Scope> DEFAULT_SCOPES = new List<Scope>
        {
            new Scope
            {
                Name = "openid",
                IsExposed = true,
                IsOpenIdScope = true,
                IsDisplayedInConsent = true,
                Description = "access to the openid scope",
                Type = ScopeType.ProtectedApi,
                Claims = new List<string> { }
            },
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
                Type = ScopeType.ResourceOwner,
                IsDisplayedInConsent = true
            },
            new Scope
            {
                Name = "scim",
                IsExposed = true,
                IsOpenIdScope = true,
                Description = "Access to the scim",
                Claims = new List<string>
                {
                    Core.Jwt.Constants.StandardResourceOwnerClaimNames.ScimId,
                    Core.Jwt.Constants.StandardResourceOwnerClaimNames.ScimLocation
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
            },
            new Scope
            {
                Name = "role",
                IsExposed = true,
                IsOpenIdScope = false,
                IsDisplayedInConsent = true,
                Description = "Access to your roles",
                Claims = new List<string>
                {
                    Core.Jwt.Constants.StandardResourceOwnerClaimNames.Role
                },
                Type = ScopeType.ResourceOwner
            },
            new Scope
            {
                Name = "register_client",
                IsExposed = false,
                IsOpenIdScope = false,
                IsDisplayedInConsent = true,
                Description = "Register a client",
                Type = ScopeType.ProtectedApi
            },
            new Scope
            {
                Name = "manage_profile",
                IsExposed = false,
                IsOpenIdScope = false,
                IsDisplayedInConsent = true,
                Description = "Manage the user's profiles",
                Type = ScopeType.ProtectedApi
            },
            new Scope
            {
                Name = "manage_account_filtering",
                IsExposed = false,
                IsOpenIdScope = false,
                IsDisplayedInConsent = true,
                Description = "Manage the account filtering",
                Type = ScopeType.ProtectedApi
            }
        };

        public DefaultScopeRepository(ICollection<Scope> scopes)
        {
            _scopes = scopes == null ? DEFAULT_SCOPES : scopes;
        }

        public Task<bool> DeleteAsync(Scope scope)
        {
            if (scope == null)
            {
                throw new ArgumentNullException(nameof(scope));
            }

            var sc = _scopes.FirstOrDefault(s => s.Name == scope.Name);
            if (sc == null)
            {
                return Task.FromResult(false);
            }

            _scopes.Remove(sc);
            return Task.FromResult(true);
        }

        public Task<ICollection<Scope>> GetAllAsync()
        {
            ICollection<Scope> res = _scopes.Select(s => s.Copy()).ToList();
            return Task.FromResult(res);
        }

        public Task<Scope> GetAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            var scope = _scopes.FirstOrDefault(s => s.Name == name);
            if (scope == null)
            {
                return Task.FromResult((Scope)null);
            }

            return Task.FromResult(scope.Copy());
        }

        public Task<bool> InsertAsync(Scope scope)
        {
            if (scope == null)
            {
                throw new ArgumentNullException(nameof(scope));
            }

            scope.CreateDateTime = DateTime.UtcNow;
            _scopes.Add(scope.Copy());
            return Task.FromResult(true);
        }

        public Task<SearchScopeResult> Search(SearchScopesParameter parameter)
        {
            if(parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            IEnumerable<Scope> result = _scopes;
            if (parameter.ScopeNames != null && parameter.ScopeNames.Any())
            {
                result = result.Where(c => parameter.ScopeNames.Any(n => c.Name.Contains(n)));
            }

            if (parameter.Types != null && parameter.Types.Any())
            {
                var scopeTypes = parameter.Types.Select(t => (ScopeType)t);
                result = result.Where(s => scopeTypes.Contains(s.Type));
            }

            var nbResult = result.Count();
            if (parameter.Order != null)
            {
                switch (parameter.Order.Target)
                {
                    case "update_datetime":
                        switch (parameter.Order.Type)
                        {
                            case OrderTypes.Asc:
                                result = result.OrderBy(c => c.UpdateDateTime);
                                break;
                            case OrderTypes.Desc:
                                result = result.OrderByDescending(c => c.UpdateDateTime);
                                break;
                        }
                        break;
                }
            }
            else
            {
                result = result.OrderByDescending(c => c.UpdateDateTime);
            }

            if (parameter.IsPagingEnabled)
            {
                result = result.Skip(parameter.StartIndex).Take(parameter.Count);
            }

            return Task.FromResult(new SearchScopeResult
            {
                Content = result.Select(r => r.Copy()),
                StartIndex = parameter.StartIndex,
                TotalResults = nbResult
            });
        }

        public Task<ICollection<Scope>> SearchByNamesAsync(IEnumerable<string> names)
        {
            if (names == null)
            {
                throw new ArgumentNullException(nameof(names));
            }

            ICollection<Scope> result = _scopes.Where(s => names.Contains(s.Name)).Select(s => s.Copy()).ToList();
            return Task.FromResult(result);
        }

        public Task<bool> UpdateAsync(Scope scope)
        {
            if (scope == null)
            {
                throw new ArgumentNullException(nameof(scope));
            }

            var sc = _scopes.FirstOrDefault(s => s.Name == scope.Name);
            if (sc == null)
            {
                return Task.FromResult(false);
            }

            sc.Claims = scope.Claims;
            sc.Description = scope.Description;
            sc.IsDisplayedInConsent = scope.IsDisplayedInConsent;
            sc.IsExposed = scope.IsExposed;
            sc.IsOpenIdScope = scope.IsOpenIdScope;
            sc.Type = scope.Type;
            sc.UpdateDateTime = DateTime.UtcNow;
            return Task.FromResult(true);
        }
    }
}
