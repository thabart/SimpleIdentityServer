using SimpleIdentityServer.Core.Jwt;

namespace SimpleIdentityServer.Manager.Core.Results
{
    public class JwsInformationResult
    {
        public JwsProtectedHeader Header { get; set; }

        public JwsPayload Payload { get; set; }

        public JsonWebKey JsonWebKey { get; set; }
    }
}
