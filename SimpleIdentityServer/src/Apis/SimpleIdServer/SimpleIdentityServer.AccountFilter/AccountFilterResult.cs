using System.Collections.Generic;

namespace SimpleIdentityServer.AccountFilter
{
    public class AccountFilterRuleResult
    {
        public AccountFilterRuleResult(string ruleName)
        {
            RuleName = ruleName;
            ErrorMessages = new List<string>();
        }

        public string RuleName { get; set; }
        public IEnumerable<string> ErrorMessages { get; set; }
        public bool IsValid { get; set; }
    }

    public class AccountFilterResult
    {
        public AccountFilterResult()
        {
            AccountFilterRules = new List<AccountFilterRuleResult>();
        }

        public bool IsValid { get; set; }
        public IEnumerable<AccountFilterRuleResult> AccountFilterRules { get; set; }
    }
}
