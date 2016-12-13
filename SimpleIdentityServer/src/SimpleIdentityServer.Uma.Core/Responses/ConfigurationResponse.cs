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
        public string Version { get; set; }        
        public List<string> AatGrantTypesSupported { get; set; }
        public List<string> AatProfilesSupported { get; set; }
        public string AuthorizationEndPoint { get; set; }
        public List<string> ClaimTokenProfilesSupported { get; set; }
        public string DynamicClientEndPoint { get; set; }
        public string IntrospectionEndPoint { get; set; }
        public string Issuer { get; set; }
        public List<string> PatGrantTypesSupported { get; set; }
        public List<string> PatProfilesSupported { get; set; }
        public string PermissionRegistrationEndPoint { get; set; }
        public string RequestingPartyClaimsEndPoint { get; set; }
        public string ResourceSetRegistrationEndPoint { get; set; }
        public string RtpEndPoint { get; set; }
        public List<string> RtpProfilesSupported { get; set; }
        public string TokenEndPoint { get; set; }
        public List<string> UmaProfilesSupported { get; set; }
        public string PolicyEndPoint { get; set; }
    }
}