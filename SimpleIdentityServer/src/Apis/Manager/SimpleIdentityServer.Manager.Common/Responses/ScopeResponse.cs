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

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SimpleIdentityServer.Manager.Common.Responses
{
    public enum ScopeResponseType
    {
        ProtectedApi,
        ResourceOwner
    }

    [DataContract]
    public class ScopeResponse
    {
        [JsonProperty(Constants.ScopeResponseNames.Name)]
        [DataMember(Name = Constants.ScopeResponseNames.Name)]
        public string Name { get; set; }

        [JsonProperty(Constants.ScopeResponseNames.Description)]
        [DataMember(Name = Constants.ScopeResponseNames.Description)]
        public string Description { get; set; }

        [JsonProperty(Constants.ScopeResponseNames.IsDisplayedInConsent)]
        [DataMember(Name = Constants.ScopeResponseNames.IsDisplayedInConsent)]
        public bool IsDisplayedInConsent { get; set; }

        [JsonProperty(Constants.ScopeResponseNames.IsOpenIdScope)]
        [DataMember(Name = Constants.ScopeResponseNames.IsOpenIdScope)]
        public bool IsOpenIdScope { get; set; }

        [JsonProperty(Constants.ScopeResponseNames.IsExposed)]
        [DataMember(Name = Constants.ScopeResponseNames.IsExposed)]
        public bool IsExposed { get; set; }

        [JsonProperty(Constants.ScopeResponseNames.Type)]
        [DataMember(Name = Constants.ScopeResponseNames.Type)]
        public ScopeResponseType Type { get; set; }

        [JsonProperty(Constants.ScopeResponseNames.Claims)]
        [DataMember(Name = Constants.ScopeResponseNames.Claims)]
        public List<string> Claims { get; set; }
    }
}
