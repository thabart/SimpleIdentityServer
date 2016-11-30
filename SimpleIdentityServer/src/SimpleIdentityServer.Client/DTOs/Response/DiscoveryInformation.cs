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

using System.Runtime.Serialization;

namespace SimpleIdentityServer.Client.DTOs.Response
{
    [DataContract]
    public sealed class DiscoveryInformation
    {
        /// <summary>
        /// Gets or sets the authorization end point.
        /// </summary>
        [DataMember(Name = Constants.DiscoveryInformationNames.AuthorizationEndPoint)]
        public string AuthorizationEndPoint { get; set; }

        [DataMember(Name = Constants.DiscoveryInformationNames.CheckSessionEndPoint)]
        public string CheckSessionEndPoint { get; set; }

        /// <summary>
        /// Gets or sets boolean specifying whether the OP supports use of the claims parameter.
        /// </summary>
        [DataMember(Name = Constants.DiscoveryInformationNames.ClaimsParameterSupported)]
        public bool ClaimsParameterSupported { get; set; }

        /// <summary>
        /// Gets or sets a list of the Claim Names of the Claims.
        /// </summary>
        [DataMember(Name = Constants.DiscoveryInformationNames.ClaimsSupported)]
        public string[] ClaimsSupported { get; set; }

        [DataMember(Name = Constants.DiscoveryInformationNames.EndSessionEndPoint)]
        public string EndSessionEndPoint { get; set; }

        /// <summary>
        /// Gets or sets the grant-types supported : authorization_code, implicit
        /// </summary>
        [DataMember(Name = Constants.DiscoveryInformationNames.GrantTypesSupported)]
        public string[] GrantTypesSupported { get; set; }

        /// <summary>
        /// Gets or sets the list of the JWS signing algorithms (alg values) supported.
        /// </summary>
        [DataMember(Name = Constants.DiscoveryInformationNames.IdTokenSigningAlgValuesSupported)]
        public string[] IdTokenSigningAlgValuesSupported { get; set; }

        /// <summary>
        /// Gets or sets the issuer. 
        /// </summary>
        [DataMember(Name = Constants.DiscoveryInformationNames.Issuer)]
        public string Issuer { get; set; }

        /// <summary>
        /// Gets or sets the JSON Web Key Set document.
        /// </summary>
        [DataMember(Name = Constants.DiscoveryInformationNames.JwksUri)]
        public string JwksUri { get; set; }

        /// <summary>
        /// Gets or sets boolean specifying whether the OP supports use of the request parameter.
        /// </summary>
        [DataMember(Name = Constants.DiscoveryInformationNames.RequestParameterSupported)]
        public bool RequestParameterSupported { get; set; }

        /// <summary>
        /// Gets or sets boolean specifying whether the OP supports use of the request request_uri
        /// </summary>
        [DataMember(Name = Constants.DiscoveryInformationNames.RequestUriParameterSupported)]
        public bool RequestUriParameterSupported { get; set; }

        /// <summary>
        /// Gets or sets boolean specifying whether the OP requires any request_uri values.
        /// </summary>
        [DataMember(Name = Constants.DiscoveryInformationNames.RequireRequestUriRegistration)]
        public bool RequireRequestUriRegistration { get; set; }

        /// <summary>
        /// Gets or sets the response modes supported : query, fragment
        /// </summary>
        [DataMember(Name = Constants.DiscoveryInformationNames.ResponseModesSupported)]
        public string[] ResponseModesSupported { get; set; }

        /// <summary>
        /// Gets or sets the response types supported : code, id_token & token id_token
        /// </summary>
        [DataMember(Name = Constants.DiscoveryInformationNames.ResponseTypesSupported)]
        public string[] ResponseTypesSupported { get; set; }

        [DataMember(Name = Constants.DiscoveryInformationNames.RevocationEndPoint)]
        public string RevocationEndPoint { get; set; }

        /// <summary>
        /// Gets or sets the list of scupported scopes.
        /// </summary>
        [DataMember(Name = Constants.DiscoveryInformationNames.ScopesSupported)]
        public string[] ScopesSupported { get; set; }

        /// <summary>
        /// Gets or sets the subject types supported : pairwise & public.
        /// </summary>
        [DataMember(Name = Constants.DiscoveryInformationNames.SubjectTypesSupported)]
        public string[] SubjectTypesSupported { get; set; }

        /// <summary>
        /// Gets or sets the token endpoint.
        /// </summary>
        [DataMember(Name = Constants.DiscoveryInformationNames.TokenEndPoint)]
        public string TokenEndPoint { get; set; }

        /// <summary>
        /// Gets or sets the list of Client Authentication methods supported by the TokenEndpoint : client_secret_post, client_secret_basic etc ...
        /// </summary>
        [DataMember(Name = Constants.DiscoveryInformationNames.TokenEndpointAuthMethodSupported)]
        public string[] TokenEndpointAuthMethodSupported { get; set; }

        /// <summary>
        /// Gets or sets the user-info endpoint.
        /// </summary>
        [DataMember(Name = Constants.DiscoveryInformationNames.UserInfoEndPoint)]
        public string UserInfoEndPoint { get; set; }

        /// <summary>
        /// Gets or sets the version of the discovery document
        /// </summary>
        [DataMember(Name = Constants.DiscoveryInformationNames.Version)]
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets the Registration End Point.
        /// </summary>
        [DataMember(Name = Constants.DiscoveryInformationNames.RegistrationEndPoint)]
        public string RegistrationEndPoint { get; set; }

        /// <summary>
        /// Gets or sets the base URI of the OP's designated SCIM service provider.
        /// </summary>
        [DataMember(Name = Constants.DiscoveryInformationNames.ScimEndPoint)]
        public string ScimEndPoint { get; set; }
    }
}
