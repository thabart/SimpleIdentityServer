using System.Collections.Generic;

namespace SimpleIdentityServer.DataAccess.SqlServer.Models
{
    public class Claim
    {
        public string Code { get; set; }

        public virtual ICollection<Scope> Scopes { get; set; }
    }
}
