using System.Collections.Generic;

namespace SimpleIdentityServer.DataAccess.SqlServer.Models
{
    public class Consent
    {
        public int Id { get; set; }

        public string ClientId { get; set; }

        public string ResourceOwnerId { get; set; } 

        public Client Client { get; set; }

        public ResourceOwner ResourceOwner { get; set; }

        public List<ConsentScope> ConsentScopes { get; set; }

        public List<ConsentClaim> ConsentClaims { get; set; }
    }
}
