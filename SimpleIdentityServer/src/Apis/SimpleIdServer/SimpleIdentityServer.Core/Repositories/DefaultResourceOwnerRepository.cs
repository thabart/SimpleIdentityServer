using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using SimpleIdentityServer.Core.Common.Models;
using SimpleIdentityServer.Core.Common.Parameters;
using SimpleIdentityServer.Core.Common.Repositories;
using SimpleIdentityServer.Core.Common.Results;
using SimpleIdentityServer.Core.Extensions;

namespace SimpleIdentityServer.Core.Repositories
{
    internal sealed class DefaultResourceOwnerRepository : IResourceOwnerRepository
    {
        public ICollection<ResourceOwner> _users;

        public DefaultResourceOwnerRepository(ICollection<ResourceOwner> users)
        {
            _users = users == null ? new List<ResourceOwner>() : users;
        }

        public Task<bool> DeleteAsync(string subject)
        {
            if (string.IsNullOrWhiteSpace(subject))
            {
                throw new ArgumentNullException(nameof(subject));
            }

            var user = _users.FirstOrDefault(u => u.Id == subject);
            if (user == null)
            {
                return Task.FromResult(false);
            }

            _users.Remove(user);
            return Task.FromResult(true);
        }

        public Task<ICollection<ResourceOwner>> GetAllAsync()
        {
            ICollection<ResourceOwner> res = _users.Select(u => u.Copy()).ToList();
            return Task.FromResult(res);
        }

        public Task<ResourceOwner> GetAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            var user = _users.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return Task.FromResult((ResourceOwner)null);
            }

            return Task.FromResult(user.Copy());
        }

        public Task<ResourceOwner> GetAsync(string id, string password)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentNullException(nameof(password));
            }

            var user = _users.FirstOrDefault(u => u.Id == id && u.Password == password);
            if (user == null)
            {
                return Task.FromResult((ResourceOwner)null);
            }

            return Task.FromResult(user.Copy());
        }

        public Task<ICollection<ResourceOwner>> GetAsync(IEnumerable<Claim> claims)
        {
            if (claims == null)
            {
                throw new ArgumentNullException(nameof(claims));
            }

            ICollection<ResourceOwner> result = _users.Where(u => claims.All(c => u.Claims.Any(sc => sc.Value == c.Value && sc.Type == c.Type)))
                .Select(u => u.Copy())
                .ToList();
            return Task.FromResult(result);
        }

        public Task<ResourceOwner> GetResourceOwnerByClaim(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentNullException(nameof(value));
            }

            var user = _users.FirstOrDefault(u => u.Claims.Any(c => c.Type == key && c.Value == value));
            if (user == null)
            {
                return Task.FromResult((ResourceOwner)null);
            }

            return Task.FromResult(user.Copy());
        }

        public Task<bool> InsertAsync(ResourceOwner resourceOwner)
        {
            if (resourceOwner == null)
            {
                throw new ArgumentNullException(nameof(resourceOwner));
            }

            resourceOwner.CreateDateTime = DateTime.UtcNow;
            _users.Add(resourceOwner.Copy());
            return Task.FromResult(true);
        }

        public Task<SearchResourceOwnerResult> Search(SearchResourceOwnerParameter parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            IEnumerable<ResourceOwner> result = _users;
            if (parameter.Subjects != null)
            {
                result = result.Where(r => parameter.Subjects.Any(s => r.Id.Contains(s)));
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

            return Task.FromResult(new SearchResourceOwnerResult
            {
                Content = result.Select(u => u.Copy()),
                StartIndex = parameter.StartIndex,
                TotalResults = nbResult
            });
        }

        public Task<bool> UpdateAsync(ResourceOwner resourceOwner)
        {
            if (resourceOwner == null)
            {
                throw new ArgumentNullException(nameof(resourceOwner));
            }

            var user = _users.FirstOrDefault(u => u.Id == resourceOwner.Id);
            if (user == null)
            {
                return Task.FromResult(false);
            }

            user.IsLocalAccount = resourceOwner.IsLocalAccount;
            user.Password = resourceOwner.Password;
            user.TwoFactorAuthentication = resourceOwner.TwoFactorAuthentication;
            user.UpdateDateTime = DateTime.UtcNow;
            user.Claims = resourceOwner.Claims;
            return Task.FromResult(true);
        }
    }
}
