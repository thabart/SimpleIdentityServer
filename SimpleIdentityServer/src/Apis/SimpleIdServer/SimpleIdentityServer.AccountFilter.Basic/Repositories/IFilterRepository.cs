using SimpleIdentityServer.AccountFilter.Basic.Aggregates;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleIdentityServer.AccountFilter.Basic.Repositories
{
    public interface IFilterRepository
    {
        Task<IEnumerable<FilterAggregate>> GetAll();
    }
}
