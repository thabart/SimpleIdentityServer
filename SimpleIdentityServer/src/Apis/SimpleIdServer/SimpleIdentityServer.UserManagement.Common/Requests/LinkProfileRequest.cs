using System.Runtime.Serialization;
using static SimpleIdentityServer.UserManagement.Common.Constants;

namespace SimpleIdentityServer.UserManagement.Common.Requests
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
