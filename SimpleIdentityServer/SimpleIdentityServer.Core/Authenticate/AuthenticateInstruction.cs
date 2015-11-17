namespace SimpleIdentityServer.Core.Authenticate
{
    public class AuthenticateInstruction
    {
        public string ClientIdFromHttpRequestBody { get; set; }

        public string ClientSecretFromHttpRequestBody { get; set; }

        public string ClientIdFromAuthorizationHeader { get; set; }

        public string ClientAssertionType { get; set; }

        public string ClientAssertion { get; set; }
    }
}
