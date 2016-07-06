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

using SimpleIdentityServer.Core.Jwt.Signature;
using SimpleIdentityServer.Core.Models;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SimpleIdentityServer.Manager.Host.DTOs.Requests
{
    [DataContract]
    public class UpdateClientRequest
    {
        [DataMember(Name = Constants.ClientNames.ClientId)]
        public string ClientId { get; set; }

        [DataMember(Name = Constants.ClientNames.RedirectUris)]
        public List<string> RedirectUris { get; set; }

        [DataMember(Name = Constants.ClientNames.ResponseTypes)]
        public List<ResponseType> ResponseTypes { get; set; }

        [DataMember(Name = Constants.ClientNames.GrantTypes)]
        public List<GrantType> GrantTypes { get; set; }

        [DataMember(Name = Constants.ClientNames.ApplicationType)]
        public ApplicationTypes? ApplicationType { get; set; }

        [DataMember(Name = Constants.ClientNames.Contacts)]
        public List<string> Contacts { get; set; }

        [DataMember(Name = Constants.ClientNames.ClientName)]
        public string ClientName { get; set; }

        [DataMember(Name = Constants.ClientNames.LogoUri)]
        public string LogoUri { get; set; }

        [DataMember(Name = Constants.ClientNames.ClientUri)]
        public string ClientUri { get; set; }

        [DataMember(Name = Constants.ClientNames.PolicyUri)]
        public string PolicyUri { get; set; }

        [DataMember(Name = Constants.ClientNames.TosUri)]
        public string TosUri { get; set; }

        [DataMember(Name = Constants.ClientNames.JwksUri)]
        public string JwksUri { get; set; }

        /// <summary>
        /// The Client Json Web Key set are passed by value
        /// </summary>
        [DataMember(Name = Constants.ClientNames.Jwks)]
        public JsonWebKeySet Jwks { get; set; }

        [DataMember(Name = Constants.ClientNames.SectoreIdentifierUri)]
        public string SectorIdentifierUri { get; set; }

        [DataMember(Name = Constants.ClientNames.SubjectType)]
        public string SubjectType { get; set; }

        [DataMember(Name = Constants.ClientNames.IdTokenSignedResponseAlg)]
        public string IdTokenSignedResponseAlg { get; set; }

        [DataMember(Name = Constants.ClientNames.IdTokenEncryptedResponseAlg)]
        public string IdTokenEncryptedResponseAlg { get; set; }

        [DataMember(Name = Constants.ClientNames.IdTokenEncryptedResponseEnc)]
        public string IdTokenEncryptedResponseEnc { get; set; }

        [DataMember(Name = Constants.ClientNames.UserInfoSignedResponseAlg)]
        public string UserInfoSignedResponseAlg { get; set; }

        [DataMember(Name = Constants.ClientNames.UserInfoEncryptedResponseAlg)]
        public string UserInfoEncryptedResponseAlg { get; set; }

        [DataMember(Name = Constants.ClientNames.UserInfoEncryptedResponseEnc)]
        public string UserInfoEncryptedResponseEnc { get; set; }

        [DataMember(Name = Constants.ClientNames.RequestObjectSigningAlg)]
        public string RequestObjectSigningAlg { get; set; }

        [DataMember(Name = Constants.ClientNames.RequestObjectEncryptionAlg)]
        public string RequestObjectEncryptionAlg { get; set; }

        [DataMember(Name = Constants.ClientNames.RequestObjectEncryptionEnc)]
        public string RequestObjectEncryptionEnc { get; set; }

        [DataMember(Name = Constants.ClientNames.TokenEndPointAuthMethod)]
        public string TokenEndPointAuthMethod { get; set; }

        [DataMember(Name = Constants.ClientNames.TokenEndPointAuthSigningAlg)]
        public string TokenEndPointAuthSigningAlg { get; set; }

        [DataMember(Name = Constants.ClientNames.DefaultMaxAge)]
        public int DefaultMaxAge { get; set; }

        [DataMember(Name = Constants.ClientNames.RequireAuthTime)]
        public bool RequireAuthTime { get; set; }

        [DataMember(Name = Constants.ClientNames.DefaultAcrValues)]
        public string DefaultAcrValues { get; set; }

        [DataMember(Name = Constants.ClientNames.InitiateLoginUri)]
        public string InitiateLoginUri { get; set; }

        [DataMember(Name = Constants.ClientNames.RequestUris)]
        public List<string> RequestUris { get; set; }

        [DataMember(Name = Constants.ClientNames.AllowedScopes)]
        public List<string> AllowedScopes { get; set; }
    }
}
