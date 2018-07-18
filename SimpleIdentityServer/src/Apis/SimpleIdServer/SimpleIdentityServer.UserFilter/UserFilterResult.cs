using System.Collections.Generic;

namespace SimpleIdentityServer.UserFilter
{
    public class UserFilterRuleResult
    {
        public UserFilterRuleResult(string ruleName)
        {
            RuleName = ruleName;
            ErrorMessages = new List<string>();
        }

        public string RuleName { get; set; }
        public IEnumerable<string> ErrorMessages { get; set; }
        public bool IsValid { get; set; }
    }

    public class UserFilterResult
    {
        public UserFilterResult()
        {
            UserFilterRules = new List<UserFilterRuleResult>();
        }

        public bool IsValid { get; set; }
        public IEnumerable<UserFilterRuleResult> UserFilterRules { get; set; }
    }
}
