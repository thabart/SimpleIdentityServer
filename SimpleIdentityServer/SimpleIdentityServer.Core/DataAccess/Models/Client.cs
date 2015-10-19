using System.Collections.Generic;

namespace SimpleIdentityServer.Core.DataAccess.Models
{
    public partial class Client
    {
        public string ClientId { get; set; }

        public ICollection<Scope> AllowedScopes { get; set; }

        public ICollection<RedirectionUrl> RedirectionUrls { get; set; }
    }
}
