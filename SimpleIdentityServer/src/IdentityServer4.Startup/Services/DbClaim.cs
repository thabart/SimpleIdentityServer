using System;

namespace IdentityServer4.Startup.Services
{
    internal class DbClaim
    {
        public DbClaim(string key, string value)
        {
            Id = Guid.NewGuid().ToString();
            Key = key;
            Value = value;
        }

        public string Id { get; set; }

        public string Key { get; set; }

        public string Value { get; set; }
    }
}
