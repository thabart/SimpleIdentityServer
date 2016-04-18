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
    public class ConfigurationResponse
    {
        /// <summary>
        /// OAUTH2.0 grant types supported by the authorization server in issuing AATs.
        /// </summary>
        [DataMember(Name = Constants.ConfigurationResponseNames.AatGrantTypesSupported)]
        public string AatGrantTypesSupported { get; set; }

        /// <summary>
        /// OAUTH2.0 access token types supported by the authorization server for AAT issuance.
        /// </summary>
        [DataMember(Name = Constants.ConfigurationResponseNames.AatProfilesSupported)]
        public string AatProfilesSupported { get; set; }

        [DataMember(Name = Constants.ConfigurationResponseNames.AuthorizationEndPoint)]
        public string AuthorizationEndPoint { get; set; }

        [DataMember(Name = Constants.ConfigurationResponseNames.ClaimTokenProfilesSupported)]
        public List<string> ClaimTokenProfilesSupported { get; set; }

        [DataMember(Name = Constants.ConfigurationResponseNames.DynamicClientEndPoint)]
        public string DynamicClientEndPoint { get; set; }

        [DataMember(Name = Constants.ConfigurationResponseNames.IntrospectionEndPoint)]
        public string IntrospectionEndPoint { get; set; }

        [DataMember(Name = Constants.ConfigurationResponseNames.Issuer)]
        public string Issuer { get; set; }

        [DataMember(Name = Constants.ConfigurationResponseNames.PatGrantTypesSupported)]
        public List<string> PatGrantTypesSupported { get; set; }

        [DataMember(Name = Constants.ConfigurationResponseNames.PatProfilesSupported)]
        public List<string> PatProfilesSupported { get; set; }

        [DataMember(Name = Constants.ConfigurationResponseNames.PermissionRegistrationEndPoint)]
        public string PermissionRegistrationEndPoint { get; set; }

        [DataMember(Name = Constants.ConfigurationResponseNames.RequestingPartyClaimsEndPoint)]
        public string RequestingPartyClaimsEndPoint { get; set; }

        [DataMember(Name = Constants.ConfigurationResponseNames.ResourceSetRegistrationEndPoint)]
        public string ResourceSetRegistrationEndPoint { get; set; }

        [DataMember(Name = Constants.ConfigurationResponseNames.RtpEndPoint)]
        public string RtpEndPoint { get; set; }

        [DataMember(Name = Constants.ConfigurationResponseNames.RtpProfilesSupported)]
        public List<string> RtpProfilesSupported { get; set; }

        [DataMember(Name = Constants.ConfigurationResponseNames.TokenEndPoint)]
        public string TokenEndPoint { get; set; }

        [DataMember(Name = Constants.ConfigurationResponseNames.UmaProfilesSupported)]
        public List<string> UmaProfilesSupported { get; set; }

        [DataMember(Name = Constants.ConfigurationResponseNames.Version)]
        public string Version { get; set; }
    }
}
