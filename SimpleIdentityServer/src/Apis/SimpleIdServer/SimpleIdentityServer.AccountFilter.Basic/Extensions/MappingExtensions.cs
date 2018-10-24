using SimpleIdentityServer.AccountFilter.Basic.Aggregates;
using SimpleIdentityServer.AccountFilter.Basic.Common;
using SimpleIdentityServer.AccountFilter.Basic.Common.Requests;
using SimpleIdentityServer.AccountFilter.Basic.Common.Responses;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleIdentityServer.AccountFilter.Basic.Extensions
{
    internal static class MappingExtensions
    {
        #region To Dtos

        public static FilterResponse ToDto(this FilterAggregate filter)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            return new FilterResponse
            {
                Id = filter.Id,
                CreateDateTime = filter.CreateDateTime,
                UpdateDateTime = filter.UpdateDateTime,
                Name = filter.Name,
                Rules = filter.Rules == null ? new List<FilterRuleResponse>() : filter.Rules.Select(r => r.ToDto())
            };
        }

        public static FilterRuleResponse ToDto(this FilterAggregateRule rule)
        {
            if (rule == null)
            {
                throw new ArgumentNullException(nameof(rule));
            }

            return new FilterRuleResponse
            {
                Id = rule.Id,
                ClaimKey = rule.ClaimKey,
                ClaimValue = rule.ClaimValue,
                Operation = (ComparisonOperationsDto)rule.Operation
            };
        }

        public static IEnumerable<FilterResponse> ToDtos(this IEnumerable<FilterAggregate> filters)
        {
            if (filters == null)
            {
                throw new ArgumentNullException(nameof(filters));
            }

            return filters.Select(f => f.ToDto());
        }

        #endregion

        #region To parameters

        public static FilterAggregate ToParameter(this AddFilterRequest addFilterRequest)
        {
            if (addFilterRequest == null)
            {
                throw new ArgumentNullException(nameof(addFilterRequest));
            }

            return new FilterAggregate
            {
                Name = addFilterRequest.Name,
                Rules = addFilterRequest.Rules == null ? new List<FilterAggregateRule>() : addFilterRequest.Rules.Select(r => r.ToParameter())
            };
        }

        public static FilterAggregate ToParameter(this UpdateFilterRequest updateFilterRequest)
        {
            if (updateFilterRequest == null)
            {
                throw new ArgumentNullException(nameof(updateFilterRequest));
            }

            return new FilterAggregate
            {
                Id = updateFilterRequest.Id,
                Name = updateFilterRequest.Name,
                Rules = updateFilterRequest.Rules == null ? new List<FilterAggregateRule>() : updateFilterRequest.Rules.Select(r => r.ToParameter())
            };
        }

        public static FilterAggregateRule ToParameter(this UpdateFilterRuleRequest updateFilterRuleRequest)
        {
            if (updateFilterRuleRequest == null)
            {
                throw new ArgumentNullException(nameof(updateFilterRuleRequest));
            }

            return new FilterAggregateRule
            {
                ClaimKey = updateFilterRuleRequest.ClaimKey,
                ClaimValue = updateFilterRuleRequest.ClaimValue,
                Operation = (ComparisonOperations)updateFilterRuleRequest.Operation
            };
        }

        public static FilterAggregateRule ToParameter(this AddFilterRuleRequest addFilterRuleRequest)
        {
            if(addFilterRuleRequest == null)
            {
                throw new ArgumentNullException(nameof(addFilterRuleRequest));
            }

            return new FilterAggregateRule
            {
                ClaimKey = addFilterRuleRequest.ClaimKey,
                ClaimValue = addFilterRuleRequest.ClaimValue,
                Operation = (ComparisonOperations)addFilterRuleRequest.Operation
            };
        }

        #endregion
    }
}
