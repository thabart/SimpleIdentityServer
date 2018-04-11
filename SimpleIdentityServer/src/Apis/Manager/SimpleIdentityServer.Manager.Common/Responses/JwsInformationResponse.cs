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
using SimpleIdentityServer.Core.Common.DTOs;
using SimpleIdentityServer.Core.Jwt;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SimpleIdentityServer.Manager.Common.Responses
{
    [DataContract]
    public class JwsInformationResponse
    {
        [JsonProperty(Constants.JwsInformationResponseNames.Header)]
        [DataMember(Name = Constants.JwsInformationResponseNames.Header)]
        public JwsProtectedHeader Header { get; set; }

        [JsonProperty(Constants.JwsInformationResponseNames.Payload)]
        [DataMember(Name = Constants.JwsInformationResponseNames.Payload)]
        public JwsPayload Payload { get; set; }

        [JsonProperty(Constants.JwsInformationResponseNames.JsonWebKey)]
        [DataMember(Name = Constants.JwsInformationResponseNames.JsonWebKey)]
        public Dictionary<string, object> JsonWebKey { get; set; }
    }
}
