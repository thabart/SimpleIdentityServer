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

namespace SimpleIdentityServer.Uma.Common.DTOs
{
    [DataContract]
    public class PolicyResponse
    {
        [DataMember(Name = PolicyNames.Id)]
        public string Id { get; set; }
        [DataMember(Name = PolicyNames.ResourceSetIds)]
        public List<string> ResourceSetIds { get; set; }
        [DataMember(Name = PolicyNames.Rules)]
        public List<PolicyRuleResponse> Rules { get; set; }
    }

    [DataContract]
    public class PolicyRuleResponse
    {
        [DataMember(Name = PolicyRuleNames.Id)]
        public string Id { get; set; }
        [DataMember(Name = PolicyRuleNames.ClientIdsAllowed)]
        public List<string> ClientIdsAllowed { get; set; }
        [DataMember(Name = PolicyRuleNames.Scopes)]
        public List<string> Scopes { get; set; }
        [DataMember(Name = PolicyRuleNames.Claims)]
        public List<PostClaim> Claims { get; set; }
        [DataMember(Name = PolicyRuleNames.IsResourceOwnerConsentNeeded)]
        public bool IsResourceOwnerConsentNeeded { get; set; }
        [DataMember(Name = PolicyRuleNames.Script)]
        public string Script { get; set; }
        [DataMember(Name = PolicyRuleNames.OpenIdProvider)]
        public string OpenIdProvider { get; set; }
    }
}
