using System.Collections.Generic;

namespace SimpleIdentityServer.DataAccess.SqlServer.Models
{
    public class Consent
    {
        public int Id { get; set; }

        public virtual Client Client { get; set; }

        public virtual ResourceOwner ResourceOwner { get; set; }

        public virtual ICollection<Scope> GrantedScopes { get; set; }

        public virtual ICollection<Claim> Claims { get; set; }
    }
}
