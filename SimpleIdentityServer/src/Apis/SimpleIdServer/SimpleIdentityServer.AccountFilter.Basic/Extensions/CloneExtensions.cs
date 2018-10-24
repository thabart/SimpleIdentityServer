using SimpleIdentityServer.AccountFilter.Basic.Aggregates;
using System.Collections.Generic;
using System.Linq;

namespace SimpleIdentityServer.AccountFilter.Basic.Extensions
{
    internal static class CloneExtensions
    {
        public static FilterAggregate Copy(this FilterAggregate filter)
        {
            return new FilterAggregate
            {
                CreateDateTime = filter.CreateDateTime,
                Id = filter.Id,
                Name = filter.Name,
                UpdateDateTime = filter.UpdateDateTime,
                Rules = filter.Rules == null ? new List<FilterAggregateRule>() : filter.Rules.Select(r => r.Copy())
            };
        }

        public static FilterAggregateRule Copy(this FilterAggregateRule rule)
        {
            return new FilterAggregateRule
            {
                ClaimKey = rule.ClaimKey,
                ClaimValue = rule.ClaimValue,
                Id = rule.Id,
                Operation = rule.Operation
            };
        }
    }
}
