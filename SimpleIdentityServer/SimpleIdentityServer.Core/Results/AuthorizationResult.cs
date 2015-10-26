namespace SimpleIdentityServer.Core.Results
{
    public enum Redirection
    {
        No,
        Authenticate,
        Consent
    }

    public class AuthorizationResult
    {
        public Redirection Redirection { get; set; }
    }
}
