using SimpleIdentityServer.Core.Models;

namespace SimpleIdentityServer.Core.Parameters
{
    public class ClientCredentialsParameter
    {
        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        public TokenEndPointAuthenticationMethods AuthenticationMethod { get; set; }
    }
}
