namespace SimpleIdentityServer.Core.Results
{
    public enum Redirection
    {
        No,
        Authorize,
        Consent
    }

    public class AuthorizationResult
    {
        public Redirection Redirection { get; set; }
    }
}
