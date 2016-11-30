#region copyright
// Copyright 2016 Habart Thierry
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

using SimpleIdentityServer.Core.Jwt.Signature;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SimpleIdentityServer.Client.DTOs
{
    [DataContract]
    public class Client
    {
        [DataMember(Name = Constants.ClientNames.RedirectUris)]
        public IEnumerable<string> RedirectUris { get; set; }
        [DataMember(Name = Constants.ClientNames.ResponseTypes)]
        public IEnumerable<string> ResponseTypes { get; set; }
        [DataMember(Name = Constants.ClientNames.GrantTypes)]
        public IEnumerable<string> GrantTypes { get; set; }
        [DataMember(Name = Constants.ClientNames.ApplicationType)]
        public string ApplicationType { get; set; }
        [DataMember(Name = Constants.ClientNames.Contacts)]
        public IEnumerable<string> Contacts { get; set; }
        [DataMember(Name = Constants.ClientNames.ClientName)]
        public string ClientName { get; set; }
        [DataMember(Name = Constants.ClientNames.LogoUri)]
        public string LogoUri { get; set; }
        public string ClientUri { get; set; }
        public string PolicyUri { get; set; }
        public string TosUri { get; set; }
        public string JwksUri { get; set; }
        public JsonWebKeySet Jwks { get; set; }
        public string SectorIdentifierUri { get; set; }
        public string SubjectType { get; set; }
        public string IdTokenSignedResponseAlg { get; set; }
        public string IdTokenEncryptedResponseAlg { get; set; }
        public string IdTokenEncryptedResponseEnc { get; set; }
        public string UserInfoSignedResponseAlg { get; set; }
        public string UserInfoEncryptedResponseAlg { get; set; }
        public string UserInfoEncryptedResponseEnc { get; set; }
        public string RequestObjectSigningAlg { get; set; }
        public string RequestObjectEncryptionAlg { get; set; }
        public string RequestObjectEncryptionEnc { get; set; }
        public string TokenEndpointAuthMethod { get; set; }
        public string TokenEndpointAuthSigningAlg { get; set; }
        public int DefaultMaxAge { get; set; }
        public bool RequireAuthTime { get; set; }
        public string DefaultAcrValues { get; set; }
        public string InitiateLoginUri { get; set; }
        public IEnumerable<string> RequestUris { get; set; }
        public bool ScimProfile { get; set; }
    }
}
