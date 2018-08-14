namespace SimpleIdentityServer.AccountFilter.Basic.Common
{
    internal static class Constants
    {
        public static class AddFilterRequestNames
        {
            public const string Name = "name";
            public const string Rules = "rules";
        }

        public static class AddFilterRuleRequestNames
        {
            public const string ClaimKey = "claim_key";
            public const string ClaimValue = "claim_value";
            public const string Operation = "op";
        }
    }
}
