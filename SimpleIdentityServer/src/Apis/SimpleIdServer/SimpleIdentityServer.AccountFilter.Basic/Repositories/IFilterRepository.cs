using SimpleIdentityServer.AccountFilter.Basic.Aggregates;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleIdentityServer.AccountFilter.Basic.Repositories
{
    public interface IFilterRepository
    {
        Task<IEnumerable<FilterAggregate>> GetAll();
        Task<string> Add(FilterAggregate filter);
        Task<bool> Delete(string filterId);
        Task<FilterAggregate> Get(string id);
        Task<bool> Update(FilterAggregate filter);
    }
}
