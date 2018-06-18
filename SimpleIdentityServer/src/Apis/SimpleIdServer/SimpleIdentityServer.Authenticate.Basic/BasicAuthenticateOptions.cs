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
            IsScimResourceAutomaticallyCreated = false;
        }

        public string ScimBaseUrl { get; set; }
        public BasicAuthenticationOptions AuthenticationOptions { get; set; }
        public bool IsScimResourceAutomaticallyCreated { get; set; }
    }
}
