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
using SimpleIdentityServer.Core.Common.DTOs;

namespace SimpleIdentityServer.Core.Results
{
    [DataContract]
    public class RegistrationResponse
    {
        [DataMember(Name = Constants.StandardRegistrationResponseNames.ClientId)]
        public string ClientId { get; set; }
        [DataMember(Name = Constants.StandardRegistrationResponseNames.ClientSecret)]
        public string ClientSecret { get; set; }

        // [DataMember(Name = Constants.StandardRegistrationResponseNames.RegistrationAccessToken)]
        // public string RegistrationAccessToken { get; set; }

        // [DataMember(Name = Constants.StandardRegistrationResponseNames.RegistrationClientUri)]
        // public string RegistrationClientUri { get; set; }

        [DataMember(Name = Constants.StandardRegistrationResponseNames.ClientIdIssuedAt)]
        public string ClientIdIssuedAt { get; set; }
        [DataMember(Name = Constants.StandardRegistrationResponseNames.ClientSecretExpiresAt)]
        public int ClientSecretExpiresAt { get; set; }

        #region Client information

        [DataMember(Name = Constants.StandardRegistrationRequestParameterNames.RedirectUris)]
        public string[] RedirectUris { get; set; }
        [DataMember(Name = Constants.StandardRegistrationRequestParameterNames.ResponseTypes)]
        public string[] ResponseTypes { get; set; }
        [DataMember(Name = Constants.StandardRegistrationRequestParameterNames.GrantTypes)]
        public string[] GrantTypes { get; set; }
        [DataMember(Name = Constants.StandardRegistrationRequestParameterNames.ApplicationType)]
        public string ApplicationType { get; set; }
        [DataMember(Name = Constants.StandardRegistrationRequestParameterNames.Contacts)]
        public string[] Contacts { get; set; }
        [DataMember(Name = Constants.StandardRegistrationRequestParameterNames.ClientName)]
        public string ClientName { get; set; }
        [DataMember(Name = Constants.StandardRegistrationRequestParameterNames.LogoUri)]
        public string LogoUri { get; set; }
        [DataMember(Name = Constants.StandardRegistrationRequestParameterNames.ClientUri)]
        public string ClientUri { get; set; }
        [DataMember(Name = Constants.StandardRegistrationRequestParameterNames.PolicyUri)]
        public string PolicyUri { get; set; }
        [DataMember(Name = Constants.StandardRegistrationRequestParameterNames.TosUri)]
        public string TosUri { get; set; }
        [DataMember(Name = Constants.StandardRegistrationRequestParameterNames.JwksUri)]
        public string JwksUri { get; set; }
        /// <summary>
        /// The Client Json Web Key set are passed by value
        /// </summary>
        [DataMember(Name = Constants.StandardRegistrationRequestParameterNames.Jwks)]
        public JsonWebKeySet Jwks { get; set; }
        [DataMember(Name = Constants.StandardRegistrationRequestParameterNames.SectoreIdentifierUri)]
        public string SectorIdentifierUri { get; set; }
        [DataMember(Name = Constants.StandardRegistrationRequestParameterNames.SubjectType)]
        public string SubjectType { get; set; }
        [DataMember(Name = Constants.StandardRegistrationRequestParameterNames.IdTokenSignedResponseAlg)]
        public string IdTokenSignedResponseAlg { get; set; }
        [DataMember(Name = Constants.StandardRegistrationRequestParameterNames.IdTokenEncryptedResponseAlg)]
        public string IdTokenEncryptedResponseAlg { get; set; }
        [DataMember(Name = Constants.StandardRegistrationRequestParameterNames.IdTokenEncryptedResponseEnc)]
        public string IdTokenEncryptedResponseEnc { get; set; }
        [DataMember(Name = Constants.StandardRegistrationRequestParameterNames.UserInfoSignedResponseAlg)]
        public string UserInfoSignedResponseAlg { get; set; }
        [DataMember(Name = Constants.StandardRegistrationRequestParameterNames.UserInfoEncryptedResponseAlg)]
        public string UserInfoEncryptedResponseAlg { get; set; }
        [DataMember(Name = Constants.StandardRegistrationRequestParameterNames.UserInfoEncryptedResponseEnc)]
        public string UserInfoEncryptedResponseEnc { get; set; }
        [DataMember(Name = Constants.StandardRegistrationRequestParameterNames.RequestObjectSigningAlg)]
        public string RequestObjectSigningAlg { get; set; }
        [DataMember(Name = Constants.StandardRegistrationRequestParameterNames.RequestObjectEncryptionAlg)]
        public string RequestObjectEncryptionAlg { get; set; }
        [DataMember(Name = Constants.StandardRegistrationRequestParameterNames.RequestObjectEncryptionEnc)]
        public string RequestObjectEncryptionEnc { get; set; }
        [DataMember(Name = Constants.StandardRegistrationRequestParameterNames.TokenEndPointAuthMethod)]
        public string TokenEndPointAuthMethod { get; set; }
        [DataMember(Name = Constants.StandardRegistrationRequestParameterNames.TokenEndPointAuthSigningAlg)]
        public string TokenEndPointAuthSigningAlg { get; set; }
        [DataMember(Name = Constants.StandardRegistrationRequestParameterNames.DefaultMaxAge)]
        public double DefaultMaxAge { get; set; }
        [DataMember(Name = Constants.StandardRegistrationRequestParameterNames.RequireAuthTime)]
        public bool RequireAuthTime { get; set; }
        [DataMember(Name = Constants.StandardRegistrationRequestParameterNames.DefaultAcrValues)]
        public string DefaultAcrValues { get; set; }
        [DataMember(Name = Constants.StandardRegistrationRequestParameterNames.InitiateLoginUri)]
        public string InitiateLoginUri { get; set; }
        [DataMember(Name = Constants.StandardRegistrationRequestParameterNames.RequestUris)]
        public List<string> RequestUris { get; set; }
        [DataMember(Name = Constants.StandardRegistrationRequestParameterNames.ScimProfile)]
        public bool ScimProfile { get; set; }

        #endregion
    }
}
