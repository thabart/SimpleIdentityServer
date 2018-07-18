using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace SimpleIdentityServer.UserFilter.Basic
{
    public class ResourceOwnerFilter : IResourceOwnerFilter
    {
        private readonly UserFilterBasicOptions _userFilterBasicOptions;

        public ResourceOwnerFilter(UserFilterBasicOptions userFilterBasicOptions)
        {
            _userFilterBasicOptions = userFilterBasicOptions;
        }

        public UserFilterResult Check(IEnumerable<Claim> claims)
        {
            if (claims == null)
            {
                throw new ArgumentNullException(nameof(claims));
            }

            var userFilterRules = new List<UserFilterRuleResult>();
            if(_userFilterBasicOptions.Rules != null)
            {
                foreach(var rule in _userFilterBasicOptions.Rules)
                {
                    var userFilterRule = new UserFilterRuleResult(rule.Name);
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
                    
                    userFilterRule.ErrorMessages = errorMessages;
                    userFilterRules.Add(userFilterRule);
                    userFilterRule.IsValid = !errorMessages.Any();
                }
            }

            return new UserFilterResult
            {
                UserFilterRules = userFilterRules,
                IsValid = userFilterRules.Any(u => u.IsValid)
            };
        }
    }
}
