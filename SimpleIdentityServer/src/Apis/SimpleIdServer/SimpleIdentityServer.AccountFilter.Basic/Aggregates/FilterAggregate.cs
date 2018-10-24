using System;
using System.Collections.Generic;

namespace SimpleIdentityServer.AccountFilter.Basic.Aggregates
{
    public sealed class FilterAggregate
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public DateTime CreateDateTime { get; set; }
        public DateTime UpdateDateTime { get; set; }
        public IEnumerable<FilterAggregateRule> Rules { get; set; }
    }
}
