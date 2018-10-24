using SimpleIdentityServer.Uma.Core.Extensions;
using SimpleIdentityServer.Uma.Core.Models;
using SimpleIdentityServer.Uma.Core.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Uma.Core.Repositories
{
    internal sealed class DefaultResourceSetRepository : IResourceSetRepository
    {
        public ICollection<ResourceSet> _resources;

        public DefaultResourceSetRepository(ICollection<ResourceSet> resources)
        {
            _resources = resources == null ? new List<ResourceSet>() : resources;
        }

        public Task<bool> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            var policy = _resources.FirstOrDefault(p => p.Id == id);
            if (policy == null)
            {
                return Task.FromResult(false);
            }

            _resources.Remove(policy);
            return Task.FromResult(true);
        }

        public Task<ResourceSet> Get(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            var rec = _resources.FirstOrDefault(p => p.Id == id);
            if (rec == null)
            {
                return Task.FromResult((ResourceSet)null);
            }

            return Task.FromResult(rec.Copy());
        }

        public Task<IEnumerable<ResourceSet>> Get(IEnumerable<string> ids)
        {
            if (ids == null)
            {
                throw new ArgumentNullException(nameof(ids));
            }

            IEnumerable<ResourceSet> result = _resources.Where(r => ids.Contains(r.Id))
                .Select(r => r.Copy())
                .ToList();
            return Task.FromResult(result);
        }

        public Task<ICollection<ResourceSet>> GetAll()
        {
            ICollection<ResourceSet> result = _resources.Select(r => r.Copy()).ToList();
            return Task.FromResult(result);
        }

        public Task<bool> Insert(ResourceSet resourceSet)
        {
            if (resourceSet == null)
            {
                throw new ArgumentNullException(nameof(resourceSet));
            }

            _resources.Add(resourceSet.Copy());
            return Task.FromResult(true);
        }

        public Task<SearchResourceSetResult> Search(SearchResourceSetParameter parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            IEnumerable<ResourceSet> result = _resources;
            if (parameter.Ids != null && parameter.Ids.Any())
            {
                result = result.Where(r => parameter.Ids.Contains(r.Id));
            }

            if (parameter.Names != null && parameter.Names.Any())
            {
                result = result.Where(r => parameter.Names.Any(n => r.Name.Contains(n)));
            }

            if (parameter.Types != null && parameter.Types.Any())
            {
                result = result.Where(r => parameter.Types.Any(t => r.Type.Contains(t)));
            }

            var nbResult = result.Count();
            result = result.OrderBy(c => c.Id);
            if (parameter.IsPagingEnabled)
            {
                result = result.Skip(parameter.StartIndex).Take(parameter.Count);
            }

            return Task.FromResult(new SearchResourceSetResult
            {
                Content = result.Select(r => r.Copy()),
                StartIndex = parameter.StartIndex,
                TotalResults = nbResult
            });
        }

        public Task<bool> Update(ResourceSet resourceSet)
        {
            if (resourceSet == null)
            {
                throw new ArgumentNullException(nameof(resourceSet));
            }

            var rec = _resources.FirstOrDefault(p => p.Id == resourceSet.Id);
            if (rec == null)
            {
                return Task.FromResult(false);
            }

            rec.AuthorizationPolicyIds = resourceSet.AuthorizationPolicyIds;
            rec.IconUri = resourceSet.IconUri;
            rec.Name = resourceSet.Name;
            rec.Policies = resourceSet.Policies;
            rec.Scopes = resourceSet.Scopes;
            rec.Type = resourceSet.Type;
            rec.Uri = resourceSet.Uri;
            return Task.FromResult(true);
        }
    }
}
