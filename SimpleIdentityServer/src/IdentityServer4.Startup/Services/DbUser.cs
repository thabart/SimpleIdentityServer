using System.Collections.Generic;

namespace IdentityServer4.Startup.Services
{
    internal class DbUser
    {
        public bool Enabled { get; set; }

        public string Password { get; set; }

        public string Provider { get; set; }

        public string ProviderId { get; set; }

        public string Subject { get; set; }

        public string Username { get; set; }

        public bool IsLocalAccount { get; set; }

        public virtual IEnumerable<DbClaim> Claims { get; set; }
    }
}
