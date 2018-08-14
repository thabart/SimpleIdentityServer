using SimpleIdentityServer.AccountFilter.Basic.Aggregates;
using SimpleIdentityServer.AccountFilter.Basic.EF.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleIdentityServer.AccountFilter.Basic.EF.Extensions
{
    internal static class MappingExtensions
    {
        #region To aggregate

        public static FilterAggregate ToAggregate(this Filter filter)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            return new FilterAggregate
            {
                Id = filter.Id,
                Name = filter.Name,
                CreateDateTime = filter.CreateDateTime,
                UpdateDateTime = filter.UpdateDateTime,
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
                Id = filterRule.Id,
                ClaimKey = filterRule.ClaimKey,
                ClaimValue = filterRule.ClaimValue,
                Operation = (ComparisonOperations)filterRule.Operation
            };
        }

        #endregion

        #region To model

        public static Filter ToModel(this FilterAggregate filter)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }
            
            return new Filter
            {
                Name = filter.Name,
                Rules = filter.Rules == null ? new List<FilterRule>() : filter.Rules.Select(r => r.ToModel()).ToList()
            };
        }

        public static FilterRule ToModel(this FilterAggregateRule filterRule)
        {
            if (filterRule == null)
            {
                throw new ArgumentNullException(nameof(filterRule));
            }

            return new FilterRule
            {
                ClaimKey = filterRule.ClaimKey,
                ClaimValue = filterRule.ClaimValue,
                Operation = (int)filterRule.Operation
            };
        }

        #endregion
    }
}
