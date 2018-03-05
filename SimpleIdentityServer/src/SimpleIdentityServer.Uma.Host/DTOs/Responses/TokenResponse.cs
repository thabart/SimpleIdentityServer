using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SimpleIdentityServer.Uma.Host.DTOs.Responses
{
    [DataContract]
    public class TokenResponse
    {
        [DataMember(Name = "access_token")]
        public string AccessToken { get; set; }
        [DataMember(Name = "id_token")]
        public string IdToken { get; set; }
        [DataMember(Name = "token_type")]
        public string TokenType { get; set; }
        [DataMember(Name = "refresh_token")]
        public string RefreshToken { get; set; }
        [DataMember(Name = "expires_in")]
        public int ExpiresIn { get; set; }
        [DataMember(Name = "scope")]
        public List<string> Scope { get; set; }
    }
}
