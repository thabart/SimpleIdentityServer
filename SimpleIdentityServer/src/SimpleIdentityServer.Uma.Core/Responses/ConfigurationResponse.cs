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

namespace SimpleIdentityServer.Uma.Core.Responses
{
    public sealed class ConfigurationResponse
    {
        public string Issuer { get; set; }
        public string RegistrationEndpoint { get; set; }
        public string TokenEndpoint { get; set; }
        public string JwksUri { get; set; }
        public string AuthorizationEndpoint { get; set; }
        public string ClaimsInteractionEndpoint { get; set; }
        public string IntrospectionEndpoint { get; set; }
        public string ResourceRegistrationEndpoint { get; set; }
        public string PermissionEndpoint { get; set; }
        public string RevocationEndpoint { get; set; }
        public List<string> ClaimTokenProfilesSupported { get; set; }
        public List<string> UmaProfilesSupported { get; set; }
        public List<string> ScopesSupported { get; set; }
        public List<string> ResponseTypesSupported { get; set; }
        public List<string> GrantTypesSupported { get; set; }
        public List<string> TokenEndpointAuthMethodsSupported { get; set; }
        public List<string> TokenEndpointAuthSigningAlgValuesSupported { get; set; }
        public List<string> UiLocalesSupported { get; set; }
    }
}