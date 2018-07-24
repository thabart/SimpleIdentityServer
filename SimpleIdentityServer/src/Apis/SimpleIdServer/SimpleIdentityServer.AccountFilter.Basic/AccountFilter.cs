using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace SimpleIdentityServer.AccountFilter.Basic
{
    public class AccountFilter : IAccountFilter
    {
        private readonly AccountFilterBasicOptions _accountFilterBasicOptions;

        public AccountFilter(AccountFilterBasicOptions accountFilterBasicOptions)
        {
            _accountFilterBasicOptions = accountFilterBasicOptions;
        }

        public AccountFilterResult Check(IEnumerable<Claim> claims)
        {
            if (claims == null)
            {
                throw new ArgumentNullException(nameof(claims));
            }

            var accountFilterRules = new List<AccountFilterRuleResult>();
            if(_accountFilterBasicOptions.Rules != null)
            {
                foreach(var rule in _accountFilterBasicOptions.Rules)
                {
                    var accountFilterRule = new AccountFilterRuleResult(rule.Name);
                    var errorMessages = new List<string>();
                    if (rule.Comparisons != null)
                    {
                        foreach(var comparison in rule.Comparisons)
                        {
                            var claim = claims.FirstOrDefault(c => c.Type == comparison.ClaimKey);
                            if (claim == null)
                            {
                                errorMessages.Add($"the claim '{comparison.ClaimKey}' doesn't exist");
                                continue;
                            }

                            switch (comparison.Operation)
                            {
                                case ComparisonOperations.Equal:
                                    if (comparison.ClaimValue != claim.Value)
                                    {
                                        errorMessages.Add($"the filter claims['{claim.Type}'] == '{comparison.ClaimValue}' is wrong");
                                    }
                                    break;
                                case ComparisonOperations.NotEqual:
                                    if (comparison.ClaimValue == claim.Value)
                                    {
                                        errorMessages.Add($"the filter claims['{claim.Type}'] != '{comparison.ClaimValue}' is wrong");
                                    }
                                    break;
                                case ComparisonOperations.RegularExpression:
                                    var regex = new Regex(comparison.ClaimValue);
                                    if (!regex.IsMatch(claim.Value))
                                    {
                                        errorMessages.Add($"the filter claims['{claim.Type}'] match regular expression {comparison.ClaimValue} is wrong");
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

            return new AccountFilterResult
            {
                AccountFilterRules = accountFilterRules,
                IsValid = accountFilterRules.Any(u => u.IsValid)
            };
        }
    }
}
