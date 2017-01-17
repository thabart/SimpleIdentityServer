#region copyright
// Copyright 2015 Habart Thierry
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System.Runtime.Serialization;

namespace SimpleIdentityServer.Core.Common.DTOs
{
    public enum GrantTypes
    {
        None,
        password,
        client_credentials,
        authorization_code,
        validate_bearer,
        refresh_token
    }

    [DataContract]
    public class TokenRequest
    {
        [DataMember(Name = RequestTokenNames.GrantType)]
        public GrantTypes GrantType { get; set; }
        [DataMember(Name = RequestTokenNames.Username)]
        public string Username { get; set; }
        [DataMember(Name = RequestTokenNames.Password)]
        public string Password { get; set; }
        [DataMember(Name = RequestTokenNames.Scope)]
        public string Scope { get; set; }
        [DataMember(Name = RequestTokenNames.Code)]
        public string Code { get; set; }
        [DataMember(Name = RequestTokenNames.RedirectUri)]
        public string RedirectUri { get; set; }
        [DataMember(Name = RequestTokenNames.RefreshToken)]
        public string RefreshToken { get; set; }
        [DataMember(Name = ClientAuthNames.ClientId)]
        public string ClientId { get; set; }
        [DataMember(Name = ClientAuthNames.ClientSecret)]
        public string ClientSecret { get; set; }
        [DataMember(Name = ClientAuthNames.ClientAssertionType)]
        public string ClientAssertionType { get; set; }
        [DataMember(Name = ClientAuthNames.ClientAssertion)]
        public string ClientAssertion { get; set; }
        [DataMember(Name = RequestTokenNames.CodeVerifier)]
        public string CodeVerifier { get; set; }
    }
}