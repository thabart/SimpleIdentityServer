using SimpleIdentityServer.EventStore.Core.Models;
using SimpleIdentityServer.EventStore.Core.Parameters;
using SimpleIdentityServer.EventStore.Core.Results;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleIdentityServer.EventStore.Core.Repositories
{
    public interface IEventAggregateRepository
    {
        Task<bool> Add(EventAggregate evtAggregate);
        Task<EventAggregate> Get(string id);
        Task<IEnumerable<EventAggregate>> GetByAggregate(string aggregateId);
        Task<SearchResult> Search(SearchParameter searchParameter);
    }
}
