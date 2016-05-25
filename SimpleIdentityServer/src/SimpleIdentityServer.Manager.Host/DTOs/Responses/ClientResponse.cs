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

using SimpleIdentityServer.Core.Jwt;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SimpleIdentityServer.Manager.Host.DTOs.Responses
{
    [DataContract]
    public class ClientResponse
    {
        /// <summary>
        /// Gets or sets the client identifier.
        /// </summary>
        [DataMember(Name = Constants.ClientNames.ClientId)]
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets the client secret.
        /// </summary>
        [DataMember(Name = Constants.ClientNames.ClientSecret)]
        public string ClientSecret { get; set; }

        /// <summary>
        /// Gets or sets the client name
        /// </summary>
        [DataMember(Name = Constants.ClientNames.ClientName)]
        public string ClientName { get; set; }

        /// <summary>
        /// Gets or sets the logo uri
        /// </summary>
        [DataMember(Name = Constants.ClientNames.LogoUri)]
        public string LogoUri { get; set; }

        /// <summary>
        /// Gets or sets the home page of the client.
        /// </summary>
        [DataMember(Name = Constants.ClientNames.ClientUri)]
        public string ClientUri { get; set; }

        /// <summary>
        /// Gets or sets the URL that the RP provides to the End-User to read about the how the profile data will be used.
        /// </summary>
        [DataMember(Name = Constants.ClientNames.PolicyUri)]
        public string PolicyUri { get; set; }

        /// <summary>
        /// Gets or sets the URL that the RP provides to the End-User to read about the RP's terms of service.
        /// </summary>
        [DataMember(Name = Constants.ClientNames.TosUri)]
        public string TosUri { get; set; }

        #region Encryption mechanism for ID TOKEN

        /// <summary>
        /// Gets or sets the JWS alg algorithm for signing the ID token issued to this client.
        /// The default is RS256. The public key for validating the signature is provided by retrieving the JWK Set referenced by the JWKS_URI
        /// </summary>
        [DataMember(Name = Constants.ClientNames.IdTokenSignedResponseAlg)]
        public string IdTokenSignedResponseAlg { get; set; }

        /// <summary>
        /// Gets or sets the JWE alg algorithm. REQUIRED for encrypting the ID token issued to this client.
        /// The default is that no encryption is performed
        /// </summary>
        [DataMember(Name = Constants.ClientNames.IdTokenEncryptedResponseAlg)]
        public string IdTokenEncryptedResponseAlg { get; set; }

        /// <summary>
        /// Gets or sets the JWE enc algorithm. REQUIRED for encrypting the ID token issued to this client.
        /// If IdTokenEncryptedResponseAlg is specified then the value is A128CBC-HS256
        /// </summary>
        [DataMember(Name = Constants.ClientNames.IdTokenEncryptedResponseEnc)]
        public string IdTokenEncryptedResponseEnc { get; set; }

        #endregion

        /// <summary>
        /// Gets or sets the client authentication method for the Token Endpoint. 
        /// </summary>
        [DataMember(Name = Constants.ClientNames.TokenEndPointAuthMethod)]
        public string TokenEndPointAuthMethod { get; set; }

        /// <summary>
        /// Gets or sets an array containing a list of OAUTH2.0 response_type values
        /// </summary>
        [DataMember(Name = Constants.ClientNames.ResponseTypes)]
        public List<string> ResponseTypes { get; set; }

        /// <summary>
        /// Gets or sets an array containing a list of OAUTH2.0 grant types
        /// </summary>
        [DataMember(Name = Constants.ClientNames.GrantTypes)]
        public List<string> GrantTypes { get; set; }

        /// <summary>
        /// Gets or sets a list of OAUTH2.0 grant_types.
        /// </summary>
        [DataMember(Name = Constants.ClientNames.AllowedScopes)]
        public List<string> AllowedScopes { get; set; }

        /// <summary>
        /// Gets or sets an array of Redirection URI values used by the client.
        /// </summary>
        [DataMember(Name = Constants.ClientNames.RedirectUris)]
        public List<string> RedirectUris { get; set; }

        /// <summary>
        /// Gets or sets the type of application
        /// </summary>
        [DataMember(Name = Constants.ClientNames.ApplicationType)]
        public string ApplicationType { get; set; }

        /// <summary>
        /// Url for the Client's JSON Web Key Set document
        /// </summary>
        [DataMember(Name = Constants.ClientNames.JwksUri)]
        public string JwksUri { get; set; }

        /// <summary>
        /// Gets or sets the list of json web keys
        /// </summary>
        [DataMember(Name = Constants.ClientNames.JsonWebKeys)]
        public List<JsonWebKey> JsonWebKeys { get; set; }

        /// <summary>
        /// Gets or sets the list of contacts
        /// </summary>
        [DataMember(Name = Constants.ClientNames.Contacts)]
        public List<string> Contacts { get; set; }

        /// <summary>
        /// Get or set the sector identifier uri
        /// </summary>
        [DataMember(Name = Constants.ClientNames.SectorIdentifierUri)]
        public string SectorIdentifierUri { get; set; }

        /// <summary>
        /// Gets or sets the subject type
        /// </summary>
        [DataMember(Name = Constants.ClientNames.SubjectType)]
        public string SubjectType { get; set; }

        /// <summary>
        /// Gets or sets the user info signed response algorithm
        /// </summary>
        [DataMember(Name = Constants.ClientNames.UserInfoSignedResponseAlg)]
        public string UserInfoSignedResponseAlg { get; set; }

        /// <summary>
        /// Gets or sets the user info encrypted response algorithm
        /// </summary>
        [DataMember(Name = Constants.ClientNames.UserInfoEncryptedResponseAlg)]
        public string UserInfoEncryptedResponseAlg { get; set; }

        /// <summary>
        /// Gets or sets the user info encrypted response enc
        /// </summary>
        [DataMember(Name = Constants.ClientNames.UserInfoEncryptedResponseEnc)]
        public string UserInfoEncryptedResponseEnc { get; set; }

        /// <summary>
        /// Gets or sets the request objects signing algorithm
        /// </summary>
        [DataMember(Name = Constants.ClientNames.RequestObjectSigningAlg)]
        public string RequestObjectSigningAlg { get; set; }

        /// <summary>
        /// Gets or sets the request object encryption algorithm
        /// </summary>
        [DataMember(Name = Constants.ClientNames.RequestObjectEncryptionAlg)]
        public string RequestObjectEncryptionAlg { get; set; }

        /// <summary>
        /// Gets or sets the request object encryption enc
        /// </summary>
        [DataMember(Name = Constants.ClientNames.RequestObjectEncryptionEnc)]
        public string RequestObjectEncryptionEnc { get; set; }

        /// <summary>
        /// Gets or sets the token endpoint authentication signing algorithm
        /// </summary>
        [DataMember(Name = Constants.ClientNames.TokenEndPointAuthSigningAlg)]
        public string TokenEndPointAuthSigningAlg { get; set; }

        /// <summary>
        /// Gets or sets the default max age
        /// </summary>
        [DataMember(Name = Constants.ClientNames.DefaultMaxAge)]
        public double DefaultMaxAge { get; set; }

        /// <summary>
        /// Gets or sets the require authentication time
        /// </summary>
        [DataMember(Name = Constants.ClientNames.RequireAuthTime)]
        public bool RequireAuthTime { get; set; }

        /// <summary>
        /// Gets or sets the default acr values
        /// </summary>
        [DataMember(Name = Constants.ClientNames.DefaultAcrValues)]
        public string DefaultAcrValues { get; set; }

        /// <summary>
        /// Gets or sets the initiate login uri
        /// </summary>
        [DataMember(Name = Constants.ClientNames.InitiateLoginUri)]
        public string InitiateLoginUri { get; set; }

        /// <summary>
        /// Gets or sets the list of request uris
        /// </summary>
        [DataMember(Name = Constants.ClientNames.RequestUris)]
        public List<string> RequestUris { get; set; }
    }
}
