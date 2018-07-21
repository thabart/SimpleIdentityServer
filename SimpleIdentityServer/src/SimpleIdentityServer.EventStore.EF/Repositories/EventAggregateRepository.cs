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
using SimpleIdentityServer.EventStore.EF.Parsers;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SimpleIdentityServer.EventStore.EF.Repositories
{
    internal class EventAggregateRepository : IEventAggregateRepository
    {
        private readonly EventStoreContext _context;
        private readonly IFilterParser _filterParser;

        public EventAggregateRepository(EventStoreContext context, IFilterParser filterParser)
        {
            _context = context;
            _filterParser = filterParser;
        }

        public async Task<bool> Add(EventAggregate evtAggregate)
        {
            try
            {
                var record = evtAggregate.ToModel();
                _context.Events.Add(record);
                await _context.SaveChangesAsync().ConfigureAwait(false);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<EventAggregate> Get(string id)
        {
            var record = await _context.Events.FirstOrDefaultAsync(e => e.Id == id).ConfigureAwait(false);
            if (record == null)
            {
                return null;
            }

            return record.ToDomain();
        }

        public async Task<IEnumerable<EventAggregate>> GetByAggregate(string aggregateId)
        {
            var records = await _context.Events.Where(e => e.AggregateId == aggregateId).Select(e => e.ToDomain()).ToListAsync().ConfigureAwait(false);
            return records;
        }

        public async Task<SearchResult> Search(SearchParameter searchParameter)
        {
            if (searchParameter == null)
            {
                throw new ArgumentNullException(nameof(searchParameter));
            }

            var filter = searchParameter.Filter;
            if (string.IsNullOrWhiteSpace(filter))
            {
                filter = "select$*";
            }
            
            var interpreter = _filterParser.Parse(filter);
            var events = interpreter.Execute(_context.Events);
            int totalResult = await events.CountAsync().ConfigureAwait(false);
            var pagedResult = events.Skip(searchParameter.StartIndex).Take(searchParameter.Count);
            return new SearchResult(totalResult, await pagedResult.ToListAsync().ConfigureAwait(false));
        }
    }
}
