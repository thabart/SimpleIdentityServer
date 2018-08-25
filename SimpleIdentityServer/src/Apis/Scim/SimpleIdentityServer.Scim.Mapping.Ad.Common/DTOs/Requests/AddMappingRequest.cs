using System.Runtime.Serialization;

namespace SimpleIdentityServer.Scim.Mapping.Ad.Common.DTOs.Requests
{
    [DataContract]
    public class AddMappingRequest
    {
        [DataMember(Name = "attribute_id")]
        public string AttributeId { get; set; }
        [DataMember(Name = "ad_property_name")]
        public string AdPropertyName { get; set; }
        [DataMember(Name = "schema_id")]
        public string SchemaId { get; set; }
    }
}
