namespace SimpleIdentityServer.Core.Models
{
    public class AuthorizationCode
    {
        public string Value { get; set; }

        public Consent Consent { get; set; }
    }
}
