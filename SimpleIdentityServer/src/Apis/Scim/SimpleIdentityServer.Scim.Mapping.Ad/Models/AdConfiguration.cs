using System.Collections.Generic;

namespace SimpleIdentityServer.Scim.Mapping.Ad.Models
{
    public class AdConfigurationSchema
    {
        public string SchemaId { get; set; }
        public string Filter { get; set; }
        public string FilterClass { get; set; }
    }

    public class AdConfiguration
    {
        public string IpAdr { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string DistinguishedName { get; set; }
        public IEnumerable<AdConfigurationSchema> AdConfigurationSchemas { get; set; }
        public bool IsEnabled { get; set; }
    }
}