using System;
using System.Runtime.Serialization;

namespace SimpleIdentityServer.Core.Models
{
    [DataContract]
    public class GrantedToken
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
        public string Scope { get; set; }

        public DateTime CreateDateTime { get; set; }
    }
}
