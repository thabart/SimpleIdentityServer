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

namespace SimpleIdentityServer.Client.DTOs.Request
{
    public enum GrantTypeRequest
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
        [DataMember(Name = Constants.TokenRequestNames.GrantType)]
        public GrantTypeRequest GrantType { get; set; }

        [DataMember(Name = Constants.TokenRequestNames.Username)]
        public string Username { get; set; }

        [DataMember(Name = Constants.TokenRequestNames.Password)]
        public string Password { get; set; }

        [DataMember(Name = Constants.TokenRequestNames.Scope)]
        public string Scope { get; set; }

        [DataMember(Name = Constants.TokenRequestNames.ClientId)]
        public string ClientId { get; set; }

        [DataMember(Name = Constants.TokenRequestNames.ClientSecret)]
        public string ClientSecret { get; set; }

        [DataMember(Name = Constants.TokenRequestNames.Code)]
        public string Code { get; set; }

        [DataMember(Name = Constants.TokenRequestNames.RedirectUri)]
        public string RedirectUri { get; set; }

        [DataMember(Name = Constants.TokenRequestNames.ClientAssertionType)]
        public string ClientAssertionType { get; set; }

        [DataMember(Name = Constants.TokenRequestNames.ClientAssertion)]
        public string ClientAssertion { get; set; }

        [DataMember(Name = Constants.TokenRequestNames.RefreshToken)]
        public string RefreshToken { get; set; }
		
		public Dictionary<string, string> GetDic()
        {
            return new Dictionary<string, string>
            {
                { Constants.TokenRequestNames.GrantType, Enum.GetName(typeof(GrantTypeRequest), GrantType) },
                { Constants.TokenRequestNames.Username, Username },
                { Constants.TokenRequestNames.Password, Password },
                { Constants.TokenRequestNames.Scope, Scope },
                { Constants.TokenRequestNames.ClientId, ClientId },
                { Constants.TokenRequestNames.ClientSecret, ClientSecret },
                { Constants.TokenRequestNames.Code, Code },
                { Constants.TokenRequestNames.RedirectUri, RedirectUri },
                { Constants.TokenRequestNames.ClientAssertionType, ClientAssertionType },
                { Constants.TokenRequestNames.ClientAssertion, ClientAssertion },
                { Constants.TokenRequestNames.RefreshToken, RefreshToken }
            };
        }
    }
}
