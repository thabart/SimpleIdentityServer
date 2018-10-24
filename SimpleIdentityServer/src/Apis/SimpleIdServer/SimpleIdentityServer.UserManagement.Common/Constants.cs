namespace SimpleIdentityServer.UserManagement.Common
{
    internal static class Constants
    {
        public static class LinkProfileRequestNames
        {
            public const string UserId = "user_id";
            public const string Issuer = "issuer";
            public const string Force = "force";
        }

        public static class LinkProfileResponseNames
        {
            public const string CreateDatetime = "create_datetime";
            public const string UpdateDatetime = "update_datetime";
        }
    }
}
