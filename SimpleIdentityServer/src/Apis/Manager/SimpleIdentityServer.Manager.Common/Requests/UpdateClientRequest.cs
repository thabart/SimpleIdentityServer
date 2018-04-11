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
using SimpleIdentityServer.Core.Models;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SimpleIdentityServer.Manager.Common.Requests
{
    [DataContract]
    public class UpdateClientRequest
    {
        [JsonProperty(Constants.ClientNames.ClientId)]
        [DataMember(Name = Constants.ClientNames.ClientId)]
        public string ClientId { get; set; }

        [JsonProperty(Constants.ClientNames.RedirectUris)]
        [DataMember(Name = Constants.ClientNames.RedirectUris)]
        public List<string> RedirectUris { get; set; }

        [JsonProperty(Constants.ClientNames.ResponseTypes)]
        [DataMember(Name = Constants.ClientNames.ResponseTypes)]
        public List<ResponseType> ResponseTypes { get; set; }

        [JsonProperty(Constants.ClientNames.GrantTypes)]
        [DataMember(Name = Constants.ClientNames.GrantTypes)]
        public List<GrantType> GrantTypes { get; set; }

        [JsonProperty(Constants.ClientNames.ApplicationType)]
        [DataMember(Name = Constants.ClientNames.ApplicationType)]
        public ApplicationTypes? ApplicationType { get; set; }

        [JsonProperty(Constants.ClientNames.Contacts)]
        [DataMember(Name = Constants.ClientNames.Contacts)]
        public List<string> Contacts { get; set; }

        [JsonProperty(Constants.ClientNames.ClientName)]
        [DataMember(Name = Constants.ClientNames.ClientName)]
        public string ClientName { get; set; }

        [JsonProperty(Constants.ClientNames.LogoUri)]
        [DataMember(Name = Constants.ClientNames.LogoUri)]
        public string LogoUri { get; set; }

        [JsonProperty(Constants.ClientNames.ClientUri)]
        [DataMember(Name = Constants.ClientNames.ClientUri)]
        public string ClientUri { get; set; }

        [JsonProperty(Constants.ClientNames.PolicyUri)]
        [DataMember(Name = Constants.ClientNames.PolicyUri)]
        public string PolicyUri { get; set; }

        [JsonProperty(Constants.ClientNames.TosUri)]
        [DataMember(Name = Constants.ClientNames.TosUri)]
        public string TosUri { get; set; }

        [JsonProperty(Constants.ClientNames.JwksUri)]
        [DataMember(Name = Constants.ClientNames.JwksUri)]
        public string JwksUri { get; set; }

        /// <summary>
        /// The Client Json Web Key set are passed by value
        /// </summary>
        [JsonProperty(Constants.ClientNames.Jwks)]
        [DataMember(Name = Constants.ClientNames.Jwks)]
        public JsonWebKeySet Jwks { get; set; }

        [JsonProperty(Constants.ClientNames.SectoreIdentifierUri)]
        [DataMember(Name = Constants.ClientNames.SectoreIdentifierUri)]
        public string SectorIdentifierUri { get; set; }

        [JsonProperty(Constants.ClientNames.SubjectType)]
        [DataMember(Name = Constants.ClientNames.SubjectType)]
        public string SubjectType { get; set; }

        [JsonProperty(Constants.ClientNames.IdTokenSignedResponseAlg)]
        [DataMember(Name = Constants.ClientNames.IdTokenSignedResponseAlg)]
        public string IdTokenSignedResponseAlg { get; set; }

        [JsonProperty(Constants.ClientNames.IdTokenEncryptedResponseAlg)]
        [DataMember(Name = Constants.ClientNames.IdTokenEncryptedResponseAlg)]
        public string IdTokenEncryptedResponseAlg { get; set; }

        [JsonProperty(Constants.ClientNames.IdTokenEncryptedResponseEnc)]
        [DataMember(Name = Constants.ClientNames.IdTokenEncryptedResponseEnc)]
        public string IdTokenEncryptedResponseEnc { get; set; }

        [JsonProperty(Constants.ClientNames.UserInfoSignedResponseAlg)]
        [DataMember(Name = Constants.ClientNames.UserInfoSignedResponseAlg)]
        public string UserInfoSignedResponseAlg { get; set; }

        [JsonProperty(Constants.ClientNames.UserInfoEncryptedResponseAlg)]
        [DataMember(Name = Constants.ClientNames.UserInfoEncryptedResponseAlg)]
        public string UserInfoEncryptedResponseAlg { get; set; }

        [JsonProperty(Constants.ClientNames.UserInfoEncryptedResponseEnc)]
        [DataMember(Name = Constants.ClientNames.UserInfoEncryptedResponseEnc)]
        public string UserInfoEncryptedResponseEnc { get; set; }

        [JsonProperty(Constants.ClientNames.RequestObjectSigningAlg)]
        [DataMember(Name = Constants.ClientNames.RequestObjectSigningAlg)]
        public string RequestObjectSigningAlg { get; set; }

        [JsonProperty(Constants.ClientNames.RequestObjectEncryptionAlg)]
        [DataMember(Name = Constants.ClientNames.RequestObjectEncryptionAlg)]
        public string RequestObjectEncryptionAlg { get; set; }

        [JsonProperty(Constants.ClientNames.RequestObjectEncryptionEnc)]
        [DataMember(Name = Constants.ClientNames.RequestObjectEncryptionEnc)]
        public string RequestObjectEncryptionEnc { get; set; }

        [JsonProperty(Constants.ClientNames.TokenEndPointAuthMethod)]
        [DataMember(Name = Constants.ClientNames.TokenEndPointAuthMethod)]
        public string TokenEndPointAuthMethod { get; set; }

        [JsonProperty(Constants.ClientNames.TokenEndPointAuthSigningAlg)]
        [DataMember(Name = Constants.ClientNames.TokenEndPointAuthSigningAlg)]
        public string TokenEndPointAuthSigningAlg { get; set; }

        [JsonProperty(Constants.ClientNames.DefaultMaxAge)]
        [DataMember(Name = Constants.ClientNames.DefaultMaxAge)]
        public int DefaultMaxAge { get; set; }

        [JsonProperty(Constants.ClientNames.RequireAuthTime)]
        [DataMember(Name = Constants.ClientNames.RequireAuthTime)]
        public bool RequireAuthTime { get; set; }

        [JsonProperty(Constants.ClientNames.DefaultAcrValues)]
        [DataMember(Name = Constants.ClientNames.DefaultAcrValues)]
        public string DefaultAcrValues { get; set; }

        [JsonProperty(Constants.ClientNames.InitiateLoginUri)]
        [DataMember(Name = Constants.ClientNames.InitiateLoginUri)]
        public string InitiateLoginUri { get; set; }

        [JsonProperty(Constants.ClientNames.RequestUris)]
        [DataMember(Name = Constants.ClientNames.RequestUris)]
        public List<string> RequestUris { get; set; }

        [JsonProperty(Constants.ClientNames.AllowedScopes)]
        [DataMember(Name = Constants.ClientNames.AllowedScopes)]
        public List<string> AllowedScopes { get; set; }
    }
}
