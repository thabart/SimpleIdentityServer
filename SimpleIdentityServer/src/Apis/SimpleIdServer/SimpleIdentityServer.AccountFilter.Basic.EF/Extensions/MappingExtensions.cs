using SimpleIdentityServer.AccountFilter.Basic.Aggregates;
using SimpleIdentityServer.AccountFilter.Basic.EF.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleIdentityServer.AccountFilter.Basic.EF.Extensions
{
    internal static class MappingExtensions
    {
        public static FilterAggregate ToAggregate(this Filter filter)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            return new FilterAggregate
            {
                Name = filter.Name,
                Rules = filter.Rules == null ? new List<FilterAggregateRule>() : filter.Rules.Select(r => r.ToAggregate())
            };
        }

        public static FilterAggregateRule ToAggregate(this FilterRule filterRule)
        {
            if (filterRule == null)
            {
                throw new ArgumentNullException(nameof(filterRule));
            }

            return new FilterAggregateRule
            {
                ClaimKey = filterRule.ClaimKey,
                ClaimValue = filterRule.ClaimValue,
                Operation = (ComparisonOperations)filterRule.Operation
            };
        }
    }
}
