namespace RpConformance
{
    public class Constants
    {
        public static class EndPoints
        {
            public const string Jwks = "jwks";
        }

        public const string BaseUrl = "https://rp.certification.openid.net:8080/simpleIdServer";
        public const string RootManagerApiUrl = "https://localhost:5444/api";
        public const string UmaConfigurationUrl = "https://localhost:5445/.well-known/uma-configuration";
        public const string BaseOpenIdUrl = "https://localhost:5443";
        public const string OpenIdConfigurationUrl = BaseOpenIdUrl + "/.well-known/openid-configuration";
        public const string CookieWebApplicationName = "CookieWebApplication";
        public const string Callback = "https://localhost:5105/Authenticate/Callback";
    }
}
