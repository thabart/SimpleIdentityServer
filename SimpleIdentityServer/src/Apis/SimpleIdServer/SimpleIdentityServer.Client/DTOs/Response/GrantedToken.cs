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

using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SimpleIdentityServer.Client.DTOs.Response
{
    [DataContract]
    public class GrantedToken
    {
        [DataMember(Name = Constants.GrantedTokenNames.AccessToken)]
        public string AccessToken { get; set; }

        [DataMember(Name = Constants.GrantedTokenNames.IdToken)]
        public string IdToken { get; set; }

        [DataMember(Name = Constants.GrantedTokenNames.TokenType)]
        public string TokenType { get; set; }

        [DataMember(Name = Constants.GrantedTokenNames.ExpiresIn)]
        public int ExpiresIn { get; set; }

        [DataMember(Name = Constants.GrantedTokenNames.RefreshToken)]
        public string RefreshToken { get; set; }

        [DataMember(Name = Constants.GrantedTokenNames.Scope)]
        public List<string> Scope { get; set; }
    }
}
