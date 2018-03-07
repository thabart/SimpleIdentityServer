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
    [DataContract]
    public class RevocationRequest
    {
        [DataMember(Name = RevocationRequestNames.Token)]
        public string Token { get; set; }
        [DataMember(Name = RevocationRequestNames.TokenTypeHint)]
        public string TokenTypeHint { get; set; }
        [DataMember(Name = ClientAuthNames.ClientId)]
        public string ClientId { get; set; }
        [DataMember(Name = ClientAuthNames.ClientSecret)]
        public string ClientSecret { get; set; }
        [DataMember(Name = ClientAuthNames.ClientAssertionType)]
        public string ClientAssertionType { get; set; }
        [DataMember(Name = ClientAuthNames.ClientAssertion)]
        public string ClientAssertion { get; set; }
    }
}
