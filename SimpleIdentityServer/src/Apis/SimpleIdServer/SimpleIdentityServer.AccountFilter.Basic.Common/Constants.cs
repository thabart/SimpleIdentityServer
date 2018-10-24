namespace SimpleIdentityServer.AccountFilter.Basic.Common
{
    internal static class Constants
    {
        public static class FilterResponseNames
        {
            public const string Id = "id";
            public const string CreateDateTime = "create_datetime";
            public const string UpdateDateTime = "update_datetime";
            public const string Name = "name";
            public const string Rules = "rules";
        }

        public static class FilterRuleResponseNames
        {
            public const string Id = "id";
            public const string ClaimKey = "claim_key";
            public const string ClaimValue = "claim_value";
            public const string Operation = "op";
        }

        public static class ComparisonOperationsDtoNames
        {
            public const string Equal = "eq";
            public const string NotEqual = "neq";
            public const string RegularExpression = "regex";
        }
    }
}
