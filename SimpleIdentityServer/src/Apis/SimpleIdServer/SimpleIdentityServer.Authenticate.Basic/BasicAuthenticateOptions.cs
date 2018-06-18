namespace SimpleIdentityServer.Authenticate.Basic
{
    public class BasicAuthenticationOptions
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string AuthorizationWellKnownConfiguration { get; set; }
    }

    public class BasicAuthenticateOptions
    {
        public BasicAuthenticateOptions()
        {
            IsExternalAccountAutomaticallyCreated = false;
        }

        public bool IsExternalAccountAutomaticallyCreated { get; set; }
        public string ScimBaseUrl { get; set; }
        public BasicAuthenticationOptions AuthenticationOptions { get; set; }
    }
}
