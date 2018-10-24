using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SimpleIdentityServer.Core.Common.DTOs.Responses
{
    [DataContract]
    public class GrantedTokenResponse
    {
        [DataMember(Name = GrantedTokenNames.AccessToken)]
        public string AccessToken { get; set; }
        [DataMember(Name = GrantedTokenNames.IdToken)]
        public string IdToken { get; set; }
        [DataMember(Name = GrantedTokenNames.TokenType)]
        public string TokenType { get; set; }
        [DataMember(Name = GrantedTokenNames.ExpiresIn)]
        public int ExpiresIn { get; set; }
        [DataMember(Name = GrantedTokenNames.RefreshToken)]
        public string RefreshToken { get; set; }
        [DataMember(Name = GrantedTokenNames.Scope)]
        public IEnumerable<string> Scope { get; set; }
    }
}
