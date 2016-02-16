namespace SimpleIdentityServer.Manager.Host
{
    public static class Constants
    {
        public static class EndPoints
        {
            public const string RootPath = "api";

            public const string Jws = RootPath + "/jws";
        }

        public static class GetJwsRequestNames
        {
            public const string Jws = "jws";

            public const string Url = "url";
        }

        public static class JwsInformationResponseNames
        {
            public const string Header = "header";

            public const string Payload = "payload";

            public const string JsonWebKey = "jsonwebkey";
        }
    }
}
