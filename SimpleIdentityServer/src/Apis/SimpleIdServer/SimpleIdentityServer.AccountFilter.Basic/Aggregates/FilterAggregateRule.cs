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
        public string Id { get; set; }
        public string ClaimKey { get; set; }
        public string ClaimValue { get; set; }
        public ComparisonOperations Operation { get; set; }
    }
}
