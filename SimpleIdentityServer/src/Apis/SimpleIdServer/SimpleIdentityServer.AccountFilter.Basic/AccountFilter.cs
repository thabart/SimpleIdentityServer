using SimpleIdentityServer.AccountFilter.Basic.Aggregates;
using SimpleIdentityServer.AccountFilter.Basic.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SimpleIdentityServer.AccountFilter.Basic
{
    public class AccountFilter : IAccountFilter
    {
        private readonly IFilterRepository _filterRepository;

        public AccountFilter(IFilterRepository filterRepository)
        {
            _filterRepository = filterRepository;
        }

        public async Task<AccountFilterResult> Check(IEnumerable<Claim> claims)
        {
            if (claims == null)
            {
                throw new ArgumentNullException(nameof(claims));
            }

            var accountFilterRules = new List<AccountFilterRuleResult>();
            var filters = await _filterRepository.GetAll();
            if (filters != null)
            {
                foreach(var filter in filters)
                {
                    var accountFilterRule = new AccountFilterRuleResult(filter.Name);
                    var errorMessages = new List<string>();
                    if (filter.Rules != null)
                    {
                        foreach(var rule in filter.Rules)
                        {
                            var claim = claims.FirstOrDefault(c => c.Type == rule.ClaimKey);
                            if (claim == null)
                            {
                                errorMessages.Add($"the claim '{rule.ClaimKey}' doesn't exist");
                                continue;
                            }

                            switch (rule.Operation)
                            {
                                case ComparisonOperations.Equal:
                                    if (rule.ClaimValue != claim.Value)
                                    {
                                        errorMessages.Add($"the filter claims['{claim.Type}'] == '{rule.ClaimValue}' is wrong");
                                    }
                                    break;
                                case ComparisonOperations.NotEqual:
                                    if (rule.ClaimValue == claim.Value)
                                    {
                                        errorMessages.Add($"the filter claims['{claim.Type}'] != '{rule.ClaimValue}' is wrong");
                                    }
                                    break;
                                case ComparisonOperations.RegularExpression:
                                    var regex = new Regex(rule.ClaimValue);
                                    if (!regex.IsMatch(claim.Value))
                                    {
                                        errorMessages.Add($"the filter claims['{claim.Type}'] match regular expression {rule.ClaimValue} is wrong");
                                    }
                                    break;
                            }
                        }
                    }
                    
                    accountFilterRule.ErrorMessages = errorMessages;
                    accountFilterRule.IsValid = !errorMessages.Any();
                    accountFilterRules.Add(accountFilterRule);
                }
            }

            if (!accountFilterRules.Any())
            {
                return new AccountFilterResult
                {
                    IsValid = true
                };
            }

            return new AccountFilterResult
            {
                AccountFilterRules = accountFilterRules,
                IsValid = accountFilterRules.Any(u => u.IsValid)
            };
        }
    }
}
