namespace SimpleIdentityServer.AccountFilter.Basic.Aggregates
{
    public enum ComparisonOperations
    {
        Equal,
        NotEqual,
        RegularExpression
    }

    public sealed class FilterAggregateRule
    {
        public string ClaimKey { get; set; }
        public string ClaimValue { get; set; }
        public ComparisonOperations Operation { get; set; }
    }
}
