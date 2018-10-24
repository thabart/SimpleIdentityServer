using System;
using System.Runtime.Serialization;
using static SimpleIdentityServer.UserManagement.Common.Constants;

namespace SimpleIdentityServer.UserManagement.Common.Responses
{
    [DataContract]
    public class ProfileResponse
    {
        [DataMember(Name = LinkProfileRequestNames.UserId)]
        public string UserId { get; set; }
        [DataMember(Name = LinkProfileRequestNames.Issuer)]
        public string Issuer { get; set; }
        [DataMember(Name = LinkProfileResponseNames.CreateDatetime)]
        public DateTime CreateDateTime { get; set; }
        [DataMember(Name = LinkProfileResponseNames.UpdateDatetime)]
        public DateTime UpdateTime { get; set; }
    }
}
