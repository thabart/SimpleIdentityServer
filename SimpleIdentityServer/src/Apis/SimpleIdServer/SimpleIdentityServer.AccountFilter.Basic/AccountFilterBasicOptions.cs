using System.Collections.Generic;

namespace SimpleIdentityServer.AccountFilter.Basic
{
    public enum ComparisonOperations
    {
        Equal,
        NotEqual,
        RegularExpression
    }

    public class FilterComparison
    {
        public string ClaimKey { get; set; }
        public string ClaimValue { get; set; }
        public ComparisonOperations Operation { get; set; }
    }

    public class FilterRule
    {
        public FilterRule()
        {
            Comparisons = new List<FilterComparison>();
        }

        public string Name { get; set; }
        public IEnumerable<FilterComparison> Comparisons { get; set; }
    }

    public class AccountFilterBasicOptions
    {
        public AccountFilterBasicOptions()
        {
            Rules = new List<FilterRule>();
        }

        public IEnumerable<FilterRule> Rules { get; set; }
    }
}