using System.Runtime.Serialization;

namespace SimpleIdentityServer.Core.Common.DTOs
{
    [DataContract]
    public class RevokeSessionRequest
    {
        [DataMember(Name = RevokeSessionRequestNames.IdTokenHint)]
        public string IdTokenHint { get; set; }
        [DataMember(Name = RevokeSessionRequestNames.PostLogoutRedirectUri)]
        public string PostLogoutRedirectUri { get; set; }
        [DataMember(Name = RevokeSessionRequestNames.State)]
        public string State { get; set; }
    }
}
