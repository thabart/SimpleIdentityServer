namespace SimpleIdentityServer.Core
{
    public class OAuthConfigurationOptions
    {
        public OAuthConfigurationOptions()
        {
            TokenValidityPeriodInSeconds = 3600;
            AuthorizationCodeValidityPeriodInSeconds = 3600;
            DefaultLanguage = "en";
        }

        public double TokenValidityPeriodInSeconds { get; set; }
        public double AuthorizationCodeValidityPeriodInSeconds { get; set; }
        public string DefaultLanguage { get; set; }
    }
}
