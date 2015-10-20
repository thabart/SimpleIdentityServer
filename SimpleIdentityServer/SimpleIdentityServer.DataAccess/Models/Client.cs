using System.Collections.Generic;

namespace SimpleIdentityServer.DataAccess.SqlServer.Models
{
    public class Client
    {
        public string ClientId { get; set; }

        public List<Scope> AllowedScopes { get; set; }

        public List<RedirectionUrl> RedirectionUrls { get; set; }
    }
}
