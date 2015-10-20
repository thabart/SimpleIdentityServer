using System.Collections.Generic;

namespace SimpleIdentityServer.Core.Models
{
    public class Client
    {
        public string ClientId { get; set; }

        public List<Scope> AllowedScopes { get; set; }

        public List<string> RedirectionUrls { get; set; }
    }
}
