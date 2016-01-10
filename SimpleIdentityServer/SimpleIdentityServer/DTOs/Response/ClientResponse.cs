using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SimpleIdentityServer.Api.DTOs.Response
{
    [DataContract]
    public class ClientResponse
    {
        [DataMember(Name = "redirect_uris")]
        public List<string> RedirectUris { get; set; }

        [DataMember(Name = "response_types")]
        public List<string> ResponseTypes { get; set; }

        [DataMember(Name = "grant_types")]
        public List<string> GrantTypes { get; set; }

        [DataMember(Name = "application_type")]
        public string ApplicationType { get; set; }

        [DataMember(Name = "contacts")]
        public List<string> Contacts { get; set; }

        [DataMember(Name = "client_name")]
        public string ClientName { get; set; }

        [DataMember(Name = "logo_uri")]
        public string LogoUri { get; set; }

        [DataMember(Name = "client_uri")]
        public string ClientUri { get; set; }

        [DataMember(Name = "policy_uri")]
        public string PolicyUri { get; set; }

        [DataMember(Name = "tos_uri")]
        public string TosUri { get; set; }

        [DataMember(Name = "jwks_uri")]
        public string JwksUri { get; set; }

        /// <summary>
        /// The Client Json Web Key set are passed by value
        /// </summary>
        [DataMember(Name = "jwks")]
        public string Jwks { get; set; }

        [DataMember(Name = "sector_identifier_uri")]
        public string SectorIdentifierUri { get; set; }

        [DataMember(Name = "subject_type")]
        public string SubjectType { get; set; }

        [DataMember(Name = "id_token_signed_response_alg")]
        public string IdTokenSignedResponseAlg { get; set; }

        [DataMember(Name = "id_token_encrypted_response_alg")]
        public string IdTokenEncryptedResponseAlg { get; set; }

        [DataMember(Name = "id_token_encrypted_response_enc")]
        public string IdTokenEncryptedResponseEnc { get; set; }

        [DataMember(Name = "userinfo_signed_response_alg")]
        public string UserInfoSignedResponseAlg { get; set; }

        [DataMember(Name = "userinfo_encrypted_response_alg")]
        public string UserInfoEncryptedResponseAlg { get; set; }

        [DataMember(Name = "userinfo_encrypted_response_enc")]
        public string UserInfoEncryptedResponseEnc { get; set; }

        [DataMember(Name = "request_object_signing_alg")]
        public string RequestObjectSigningAlg { get; set; }

        [DataMember(Name = "request_object_encryption_alg")]
        public string RequestObjectEncryptionAlg { get; set; }

        [DataMember(Name = "request_object_encryption_enc")]
        public string RequestObjectEncryptionEnc { get; set; }

        [DataMember(Name = "token_endpoint_auth_method")]
        public string TokenEndPointAuthMethod { get; set; }

        [DataMember(Name = "token_endpoint_auth_signing_alg")]
        public string TokenEndPointAuthSigningAlg { get; set; }

        [DataMember(Name = "default_max_age")]
        public string DefaultMaxAge { get; set; }

        [DataMember(Name = "require_auth_time")]
        public string RequireAuthTime { get; set; }

        [DataMember(Name = "default_acr_values")]
        public string DefaultAcrValues { get; set; }

        [DataMember(Name = "initiate_login_uri")]
        public string InitiateLoginUri { get; set; }

        [DataMember(Name = "request_uris")]
        public List<string> RequestUris { get; set; }
    }
}