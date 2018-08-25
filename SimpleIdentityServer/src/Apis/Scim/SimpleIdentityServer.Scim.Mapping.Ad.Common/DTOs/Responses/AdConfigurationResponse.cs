using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SimpleIdentityServer.Scim.Mapping.Ad.Common.DTOs.Responses
{
    [DataContract]
    public class AdConfigurationSchemaResponse
    {
        [DataMember(Name = "schema_id")]
        public string SchemaId { get; set; }
        [DataMember(Name = "filter")]
        public string Filter { get; set; }
        [DataMember(Name = "filter_class")]
        public string FilterClass { get; set; }
    }

    [DataContract]
    public class AdConfigurationResponse
    {
        [DataMember(Name = "ip_adr")]
        public string IpAdr { get; set; }
        [DataMember(Name = "port")]
        public int Port { get; set; }
        [DataMember(Name = "username")]
        public string Username { get; set; }
        [DataMember(Name = "password")]
        public string Password { get; set; }
        [DataMember(Name = "distinguished_name")]
        public string DistinguishedName { get; set; }
        [DataMember(Name = "is_enabled")]
        public bool IsEnabled { get; set; }
        [DataMember(Name = "schemas")]
        public IEnumerable<AdConfigurationSchemaResponse> Schemas { get; set; }
    }
}
