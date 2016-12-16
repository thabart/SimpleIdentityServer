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
    public class ConfigurationResponse
    {
        [DataMember(Name = ConfigurationResponseNames.AatGrantTypesSupported)]
        public List<string> AatGrantTypesSupported { get; set; }

        [DataMember(Name = ConfigurationResponseNames.AatProfilesSupported)]
        public List<string> AatProfilesSupported { get; set; }

        [DataMember(Name = ConfigurationResponseNames.AuthorizationEndPoint)]
        public string AuthorizationEndPoint { get; set; }

        [DataMember(Name = ConfigurationResponseNames.ClaimTokenProfilesSupported)]
        public List<string> ClaimTokenProfilesSupported { get; set; }

        [DataMember(Name = ConfigurationResponseNames.DynamicClientEndPoint)]
        public string DynamicClientEndPoint { get; set; }

        [DataMember(Name = ConfigurationResponseNames.IntrospectionEndPoint)]
        public string IntrospectionEndPoint { get; set; }

        [DataMember(Name = ConfigurationResponseNames.Issuer)]
        public string Issuer { get; set; }

        [DataMember(Name = ConfigurationResponseNames.PatGrantTypesSupported)]
        public List<string> PatGrantTypesSupported { get; set; }

        [DataMember(Name = ConfigurationResponseNames.PatProfilesSupported)]
        public List<string> PatProfilesSupported { get; set; }

        [DataMember(Name = ConfigurationResponseNames.PermissionRegistrationEndPoint)]
        public string PermissionRegistrationEndPoint { get; set; }

        [DataMember(Name = ConfigurationResponseNames.RequestingPartyClaimsEndPoint)]
        public string RequestingPartyClaimsEndPoint { get; set; }

        [DataMember(Name = ConfigurationResponseNames.ResourceSetRegistrationEndPoint)]
        public string ResourceSetRegistrationEndPoint { get; set; }

        [DataMember(Name = ConfigurationResponseNames.RptEndPoint)]
        public string RptEndPoint { get; set; }

        [DataMember(Name = ConfigurationResponseNames.RptProfilesSupported)]
        public List<string> RptProfilesSupported { get; set; }

        [DataMember(Name = ConfigurationResponseNames.TokenEndPoint)]
        public string TokenEndPoint { get; set; }

        [DataMember(Name = ConfigurationResponseNames.UmaProfilesSupported)]
        public List<string> UmaProfilesSupported { get; set; }

        [DataMember(Name = ConfigurationResponseNames.Version)]
        public string Version { get; set; }

        [DataMember(Name = ConfigurationResponseNames.PolicyEndPoint)]
        public string PolicyEndPoint { get; set; }
        [DataMember(Name = ConfigurationResponseNames.ScopeEndPoint)]
        public string ScopeEndPoint { get; set; }
    }
}
