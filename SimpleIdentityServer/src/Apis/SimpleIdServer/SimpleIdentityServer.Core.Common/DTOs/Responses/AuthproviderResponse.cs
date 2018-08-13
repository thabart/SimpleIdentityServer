using System.Runtime.Serialization;

namespace SimpleIdentityServer.Core.Common.DTOs.Responses
{
    [DataContract]
    public class AuthproviderResponse
    {
        [DataMember(Name = "display_name")]
        public string DisplayName { get; set; }
        [DataMember(Name = "authentication_scheme")]
        public string AuthenticationScheme { get; set; }
    }
}
