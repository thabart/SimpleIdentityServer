using SimpleIdentityServer.ResourceManager.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleIdentityServer.ResourceManager.Core.Repositories
{
    public interface IIdProviderRepository
    {
        Task<IdProviderAggregate> Get(string url);
        Task<bool> Add(IEnumerable<IdProviderAggregate> idProviders);
        Task<IEnumerable<IdProviderAggregate>> GetAll();
    }
}
