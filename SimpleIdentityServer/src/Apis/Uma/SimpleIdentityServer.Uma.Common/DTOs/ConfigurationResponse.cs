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
        [DataMember(Name = ConfigurationResponseNames.Issuer)]
        public string Issuer { get; set; }
        [DataMember(Name = ConfigurationResponseNames.RegistrationEndpoint)]
        public string RegistrationEndpoint { get; set; }
        [DataMember(Name = ConfigurationResponseNames.TokenEndpoint)]
        public string TokenEndpoint { get; set; }
        [DataMember(Name = ConfigurationResponseNames.JwksUri)]
        public string JwksUri { get; set; }
        [DataMember(Name = ConfigurationResponseNames.AuthorizationEndpoint)]
        public string AuthorizationEndpoint { get; set; }
        [DataMember(Name = ConfigurationResponseNames.ClaimsInteractionEndpoint)]
        public string ClaimsInteractionEndpoint { get; set; }
        [DataMember(Name = ConfigurationResponseNames.IntrospectionEndpoint)]
        public string IntrospectionEndpoint { get; set; }
        [DataMember(Name = ConfigurationResponseNames.ResourceRegistrationEndpoint)]
        public string ResourceRegistrationEndpoint { get; set; }
        [DataMember(Name = ConfigurationResponseNames.PermissionEndpoint)]
        public string PermissionEndpoint { get; set; }
        [DataMember(Name = ConfigurationResponseNames.RevocationEndpoint)]
        public string RevocationEndpoint { get; set; }
        [DataMember(Name = ConfigurationResponseNames.PoliciesEndpoint)]
        public string PoliciesEndpoint { get; set; }
        [DataMember(Name = ConfigurationResponseNames.ClaimTokenProfilesSupported)]
        public List<string> ClaimTokenProfilesSupported { get; set; }
        [DataMember(Name = ConfigurationResponseNames.UmaProfilesSupported)]
        public List<string> UmaProfilesSupported { get; set; }
        [DataMember(Name = ConfigurationResponseNames.ScopesSupported)]
        public List<string> ScopesSupported { get; set; }
        [DataMember(Name = ConfigurationResponseNames.ResponseTypesSupported)]
        public List<string> ResponseTypesSupported { get; set; }
        [DataMember(Name = ConfigurationResponseNames.GrantTypesSupported)]
        public List<string> GrantTypesSupported { get; set; }
        [DataMember(Name = ConfigurationResponseNames.TokenEndpointAuthMethodsSupported)]
        public List<string> TokenEndpointAuthMethodsSupported { get; set; }
        [DataMember(Name = ConfigurationResponseNames.TokenEndpointAuthSigningAlgValuesSupported)]
        public List<string> TokenEndpointAuthSigningAlgValuesSupported { get; set; }
        [DataMember(Name = ConfigurationResponseNames.UiLocalesSupported)]
        public List<string> UiLocalesSupported { get; set; }
    }
}
