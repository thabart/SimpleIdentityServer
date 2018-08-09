using Microsoft.EntityFrameworkCore;
using SimpleIdentityServer.AccountFilter.Basic.Aggregates;
using SimpleIdentityServer.AccountFilter.Basic.EF.Extensions;
using SimpleIdentityServer.AccountFilter.Basic.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdentityServer.AccountFilter.Basic.EF.Repositories
{
    internal sealed class FilterRepository : IFilterRepository
    {
        private readonly AccountFilterBasicServerContext _context;

        public FilterRepository(AccountFilterBasicServerContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<FilterAggregate>> GetAll()
        {
            var filters = _context.Filters.Include(f => f.Rules);
            var result = await filters.ToListAsync().ConfigureAwait(false);
            return result.Select(r => r.ToAggregate());
        }
    }
}
