using System.Runtime.Serialization;

namespace SimpleIdentityServer.Core.Common.DTOs.Requests
{
    [DataContract]
    public sealed class LinkProfileRequest
    {
        [DataMember(Name = LinkProfileRequestNames.UserId)]
        public string UserId { get; set; }
        [DataMember(Name = LinkProfileRequestNames.Issuer)]
        public string Issuer { get; set; }
        [DataMember(Name = LinkProfileRequestNames.Force)]
        public bool Force { get; set; }
    }
}
