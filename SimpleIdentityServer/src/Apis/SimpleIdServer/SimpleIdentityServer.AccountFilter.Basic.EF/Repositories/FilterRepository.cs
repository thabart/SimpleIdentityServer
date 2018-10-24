using Microsoft.EntityFrameworkCore;
using SimpleIdentityServer.AccountFilter.Basic.Aggregates;
using SimpleIdentityServer.AccountFilter.Basic.EF.Extensions;
using SimpleIdentityServer.AccountFilter.Basic.Repositories;
using System;
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

        public async Task<bool> Update(FilterAggregate filter)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            var record = await _context.Filters.Include(f => f.Rules).FirstOrDefaultAsync(f => f.Id == filter.Id).ConfigureAwait(false);
            if (record == null)
            {
                return false;
            }

            record.Name = filter.Name;
            record.UpdateDateTime = DateTime.UtcNow;
            record.Rules.Clear();
            if (filter.Rules != null)
            {
                foreach(var rule in filter.Rules)
                {
                    var newRule = rule.ToModel();
                    newRule.Id = Guid.NewGuid().ToString();
                    record.Rules.Add(newRule);
                }
            }

            await _context.SaveChangesAsync().ConfigureAwait(false);
            return true;
        }

        public async Task<string> Add(FilterAggregate filter)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            var record = filter.ToModel();
            record.Id = Guid.NewGuid().ToString();
            record.CreateDateTime = DateTime.UtcNow;
            record.UpdateDateTime = DateTime.UtcNow;
            if (record.Rules != null)
            {
                foreach(var rule in record.Rules)
                {
                    rule.Id = Guid.NewGuid().ToString();
                }
            }

            _context.Filters.Add(record);
            await _context.SaveChangesAsync().ConfigureAwait(false);
            return record.Id;
        }

        public async Task<bool> Delete(string filterId)
        {
            if(string.IsNullOrWhiteSpace(filterId))
            {
                throw new ArgumentNullException(nameof(filterId));
            }

            var record = await _context.Filters.FirstOrDefaultAsync(f => f.Id == filterId).ConfigureAwait(false);
            if (record == null)
            {
                return false;
            }

            _context.Filters.Remove(record);
            await _context.SaveChangesAsync().ConfigureAwait(false);
            return true;
        }

        public async Task<FilterAggregate> Get(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            var result = await _context.Filters.Include(f => f.Rules).FirstOrDefaultAsync(f => f.Id == id).ConfigureAwait(false);
            if (result == null)
            {
                return null;
            }

            return result.ToAggregate();
        }
    }
}
