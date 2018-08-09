using System.Collections.Generic;

namespace SimpleIdentityServer.AccountFilter.Basic.Aggregates
{
    public sealed class FilterAggregate
    {
        public string Name { get; set; }
        public IEnumerable<FilterAggregateRule> Rules { get; set; }
    }
}
