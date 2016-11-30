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

namespace SimpleIdentityServer.Core.Results
{
    [DataContract]
    public class DiscoveryInformation
    {
        /// <summary>
        /// Gets or sets the authorization end point.
        /// </summary>
        [DataMember(Name = "authorization_endpoint")]
        public string AuthorizationEndPoint { get; set; }

        [DataMember(Name = "check_session_iframe")]
        public string CheckSessionEndPoint { get; set; }

        /// <summary>
        /// Gets or sets the list of the Claim Types supported.
        /// </summary>
        //  [DataMember(Name = "claim_types_supported")]
        public string[] ClaimTypesSupported { get; set; }

        /// <summary>
        /// Gets or sets boolean specifying whether the OP supports use of the claims parameter.
        /// </summary>
        [DataMember(Name = "claims_parameter_supported")]
        public bool ClaimsParameterSupported { get; set; }

        /// <summary>
        /// Gets or sets a list of the Claim Names of the Claims.
        /// </summary>
        [DataMember(Name = "claims_supported")]
        public string[] ClaimsSupported { get; set; }
        
        [DataMember(Name = "end_session_endpoint")]
        public string EndSessionEndPoint { get; set; }

        /// <summary>
        /// Gets or sets the grant-types supported : authorization_code, implicit
        /// </summary>
        [DataMember(Name = "grant_types_supported")]
        public string[] GrantTypesSupported { get; set; }

        /// <summary>
        /// Gets or sets the list of the JWS signing algorithms (alg values) supported.
        /// </summary>
        [DataMember(Name = "id_token_signing_alg_values_supported")]
        public string[] IdTokenSigningAlgValuesSupported { get; set; }

        /// <summary>
        /// Gets or sets the issuer. 
        /// </summary>
        [DataMember(Name = "issuer")]
        public string Issuer { get; set; }

        /// <summary>
        /// Gets or sets the JSON Web Key Set document.
        /// </summary>
        [DataMember(Name = "jwks_uri")]
        public string JwksUri { get; set; }

        /// <summary>
        /// Gets or sets boolean specifying whether the OP supports use of the request parameter.
        /// </summary>
        [DataMember(Name = "request_parameter_supported")]
        public bool RequestParameterSupported { get; set; }

        /// <summary>
        /// Gets or sets boolean specifying whether the OP supports use of the request request_uri
        /// </summary>
        [DataMember(Name = "request_uri_parameter_supported")]
        public bool RequestUriParameterSupported { get; set; }

        /// <summary>
        /// Gets or sets boolean specifying whether the OP requires any request_uri values.
        /// </summary>
        [DataMember(Name = "require_request_uri_registration")]
        public bool RequireRequestUriRegistration { get; set; }

        /// <summary>
        /// Gets or sets the response modes supported : query, fragment
        /// </summary>
        [DataMember(Name = "response_modes_supported")]
        public string[] ResponseModesSupported { get; set; }

        /// <summary>
        /// Gets or sets the response types supported : code, id_token & token id_token
        /// </summary>
        [DataMember(Name = "response_types_supported")]
        public string[] ResponseTypesSupported { get; set; }

        [DataMember(Name = "revocation_endpoint")]
        public string RevocationEndPoint { get; set; }

        [DataMember(Name = "introspection_endpoint")]
        public string IntrospectionEndPoint { get; set; }

        /// <summary>
        /// Gets or sets the list of scupported scopes.
        /// </summary>
        [DataMember(Name = "scopes_supported")]
        public string[] ScopesSupported { get; set; }

        /// <summary>
        /// Gets or sets the subject types supported : pairwise & public.
        /// </summary>
        [DataMember(Name = "subject_types_supported")]
        public string[] SubjectTypesSupported { get; set; }

        /// <summary>
        /// Gets or sets the token endpoint.
        /// </summary>
        [DataMember(Name = "token_endpoint")]
        public string TokenEndPoint { get; set; }

        /// <summary>
        /// Gets or sets the list of Client Authentication methods supported by the TokenEndpoint : client_secret_post, client_secret_basic etc ...
        /// </summary>
        [DataMember(Name = "token_endpoint_auth_methods_supported")]
        public string[] TokenEndpointAuthMethodSupported { get; set; }

        /// <summary>
        /// Gets or sets the user-info endpoint.
        /// </summary>
        [DataMember(Name = "userinfo_endpoint")]
        public string UserInfoEndPoint { get; set; }

        /// <summary>
        /// Gets or sets the version of the discovery document
        /// </summary>
        [DataMember(Name = "version")]
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets the Registration End Point.
        /// </summary>
        [DataMember(Name = "registration_endpoint")]
        public string RegistrationEndPoint { get; set; }

        /// <summary>
        /// Gets or sets the base URI of the OP's designated SCIM service provider.
        /// </summary>
        [DataMember(Name = "scim_endpoint")]
        public string ScimEndpoint { get; set; }

        /// <summary>
        /// Gets or sets the acr values supported.
        /// </summary>
        public string[] AcrValuesSupported { get; set; }

        /// <summary>
        /// Gets or sets the list of the JWE encryption algorithms (alg values)
        /// </summary>
        public string[] IdTokenEncryptionAlgValuesSupported { get; set; }

        /// <summary>
        /// Gets or sets the list of the JWE encryption algorithms (enc values)
        /// </summary>
        public string[] IdTokenEncryptionEncValuesSupported { get; set; }

        /// <summary>
        /// Gets or sets the list of the JWS signing algorithms (alg values) supported by the UserInfo endpoint.
        /// </summary>
        public string[] UserInfoSigningAlgValuesSupported { get; set; }

        /// <summary>
        /// Gets or sets the list of the JWE encryption algorithms (alg values) supported by the UserInfo endpoint.
        /// </summary>
        public string[] UserInfoEncryptionAlgValuesSupported { get; set; }

        /// <summary>
        /// Gets or sets the list of the JWE encryption algorithms (enc values) supported by the UserInfo endpoint.
        /// </summary>
        public string[] UserInfoEncryptionEncValuesSupported { get; set; }

        /// <summary>
        /// Gets or sets the list of the JWS signing algorithms (alg values) supported by the OP for Request objects.
        /// </summary>
        public string[] RequestObjectSigningAlgValuesSupported { get; set; }

        /// <summary>
        /// Gets or sets the list of the JWE encryption algorithms (alg values) supported by the OP for Request objects.
        /// </summary>
        public string[] RequestObjectEncryptionAlgValuesSupported { get; set; }

        /// <summary>
        /// Gets or sets the list of the JWE encryption algorithms (enc values) supported by the OP for Request objects.
        /// </summary>
        public string[] RequestObjectEncryptionEncValuesSupported { get; set; }

        /// <summary>
        /// Gets or sets the list of the JWS algorithms (alg values) suppported by the Token Endpoint for the signature on the JWT.
        /// </summary>
        public string[] TokenEndpointAuthSigningAlgValuesSupported { get; set; }

        /// <summary>
        /// Gets or sets a list of display parameter values.
        /// </summary>
        public string[] DisplayValuesSupported { get; set; }

        /// <summary>
        /// Gets or sets the service documentation.
        /// </summary>
        public string ServiceDocumentation { get; set; }

        /// <summary>
        /// Gets or sets the languages & scripts supported for values in Claims being returned.
        /// </summary>
        public string[] ClaimsLocalesSupported { get; set; }

        /// <summary>
        /// Gets or sets the languages & scripts supported for the UI.
        /// </summary>
        public string[] UiLocalesSupported { get; set; }

        /// <summary>
        /// Gets or sets the OP policy.
        /// </summary>
        public string OpPolicyUri { get; set; }

        /// <summary>
        /// Gets or sets the TOS uri.
        /// </summary>
        public string OpTosUri { get; set; }
    }
}