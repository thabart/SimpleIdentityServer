using System.Runtime.Serialization;

namespace SimpleIdentityServer.Scim.Mapping.Ad.Common.DTOs.Requests
{
    [DataContract]
    public class UpdateAdConfigurationRequest
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
        [DataMember(Name = "user_filter")]
        public string UserFilter { get; set; }
        [DataMember(Name = "is_enabled")]
        public bool IsEnabled { get; set; }
    }
}
