#region copyright
// Copyright 2017 Habart Thierry
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

using Microsoft.EntityFrameworkCore;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.EventStore.EF.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdentityServer.EventStore.EF.Repositories
{
    internal class EventAggregateRepository : IEventAggregateRepository
    {
        private readonly EventStoreContext _context;

        public EventAggregateRepository(EventStoreContext context)
        {
            _context = context;
        }

        public async Task<bool> Add(EventAggregate evtAggregate)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync().ConfigureAwait(false))
            {
                try
                {
                    var record = evtAggregate.ToModel();
                    _context.Events.Add(record);
                    await _context.SaveChangesAsync().ConfigureAwait(false);
                    transaction.Commit();
                    return true;
                }
                catch
                {
                    transaction.Rollback();
                    return false;
                }
            }
        }

        public async Task<EventAggregate> Get(string id)
        {
            var record = await _context.Events.FirstOrDefaultAsync(e => e.Id == id);
            if (record == null)
            {
                return null;
            }

            return record.ToDomain();
        }

        public async Task<SearchEventAggregatesResult> Search(SearchParameter searchParameter)
        {
            if (searchParameter == null)
            {
                throw new ArgumentNullException(nameof(searchParameter));
            }


            IQueryable<Models.EventAggregate> events = _context.Events;

            if (!string.IsNullOrWhiteSpace(searchParameter.Filter))
            {
                events = events.Where(searchParameter.Filter);
            }

            if (!string.IsNullOrWhiteSpace(searchParameter.SortBy))
            {
                if (searchParameter.SortOrder == SortOrders.Ascending)
                {
                    events = events.OrderBy(searchParameter.SortBy);
                }
                else
                {
                    events = events.OrderByDescending(searchParameter.SortBy);
                }
            }

            if (!string.IsNullOrWhiteSpace(searchParameter.GroupBy))
            {
                var grouped = events.GroupBy(searchParameter.GroupBy);
                var totalResults = await grouped.CountAsync().ConfigureAwait(false);
                var pagedGroupingResult = grouped.Skip(searchParameter.StartIndex).Take(searchParameter.Count);
                var groupedEvtAggregates = new List<GroupedEventAggregate>();
                foreach (var kvp in pagedGroupingResult)
                {
                    // groupedEvtAggregates.Add(new GroupedEventAggregate(kvp.Key, kvp.Select(k => k.ToDomain())));
                }

                return new SearchEventAggregatesResult(totalResults, groupedEvtAggregates);
            }

            int totalResult = await events.CountAsync().ConfigureAwait(false);
            var pagedResult = events.Skip(searchParameter.StartIndex).Take(searchParameter.Count);
            return new SearchEventAggregatesResult(totalResult, await pagedResult.Select(e => e.ToDomain()).ToListAsync().ConfigureAwait(false));
        }
    }
}
