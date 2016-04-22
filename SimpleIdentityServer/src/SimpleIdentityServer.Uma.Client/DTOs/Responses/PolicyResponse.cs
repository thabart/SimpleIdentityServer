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

namespace SimpleIdentityServer.Client.DTOs.Responses
{
    [DataContract]
    public class PolicyResponse
    {
        [DataMember(Name = Constants.PolicyNames.Id)]
        public string Id { get; set; }

        [DataMember(Name = Constants.PolicyNames.ClientIdsAllowed)]
        public List<string> ClientIdsAllowed { get; set; }

        [DataMember(Name = Constants.PolicyNames.Scopes)]
        public List<string> Scopes { get; set; }

        [DataMember(Name = Constants.PolicyNames.IsResourceOwnerConsentNeeded)]
        public bool IsResourceOwnerConsentNeeded { get; set; }

        [DataMember(Name = Constants.PolicyNames.IsCustom)]
        public bool IsCustom { get; set; }

        [DataMember(Name = Constants.PolicyNames.Script)]
        public string Script { get; set; }

        [DataMember(Name = Constants.PolicyNames.ResourceSetIds)]
        public List<string> ResourceSetIds { get; set; }
    }
}
