namespace SimpleIdentityServer.Core.Parameters
{
    public class AuthenticationParameter
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string WellKnownAuthorizationUrl { get; set; }
    }
}
