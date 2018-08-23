using System.Runtime.Serialization;

namespace SimpleIdentityServer.Scim.Mapping.Ad.Common.DTOs.Responses
{
    [DataContract]
    public class MappingResponse
    {
        [DataMember(Name = "attribute_id")]
        public string AttributeId { get; set; }
        [DataMember(Name = "ad_property_name")]
        public string AdPropertyName { get; set; }
    }
}
