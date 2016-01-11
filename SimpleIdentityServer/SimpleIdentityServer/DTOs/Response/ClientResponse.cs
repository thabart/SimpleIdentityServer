using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SimpleIdentityServer.Api.DTOs.Response
{
    [DataContract]
    public class ClientResponse
    {
        [DataMember(Name = Core.Constants.StandardRegistrationRequestParameterNames.RedirectUris)]
        public List<string> RedirectUris { get; set; }

        [DataMember(Name = Core.Constants.StandardRegistrationRequestParameterNames.ResponseTypes)]
        public List<string> ResponseTypes { get; set; }

        [DataMember(Name = Core.Constants.StandardRegistrationRequestParameterNames.GrantTypes)]
        public List<string> GrantTypes { get; set; }

        [DataMember(Name = Core.Constants.StandardRegistrationRequestParameterNames.ApplicationType)]
        public string ApplicationType { get; set; }

        [DataMember(Name = Core.Constants.StandardRegistrationRequestParameterNames.Contacts)]
        public List<string> Contacts { get; set; }

        [DataMember(Name = Core.Constants.StandardRegistrationRequestParameterNames.ClientName)]
        public string ClientName { get; set; }

        [DataMember(Name = Core.Constants.StandardRegistrationRequestParameterNames.LogoUri)]
        public string LogoUri { get; set; }

        [DataMember(Name = Core.Constants.StandardRegistrationRequestParameterNames.ClientUri)]
        public string ClientUri { get; set; }

        [DataMember(Name = Core.Constants.StandardRegistrationRequestParameterNames.PolicyUri)]
        public string PolicyUri { get; set; }

        [DataMember(Name = Core.Constants.StandardRegistrationRequestParameterNames.TosUri)]
        public string TosUri { get; set; }

        [DataMember(Name = Core.Constants.StandardRegistrationRequestParameterNames.JwksUri)]
        public string JwksUri { get; set; }

        /// <summary>
        /// The Client Json Web Key set are passed by value
        /// </summary>
        [DataMember(Name = Core.Constants.StandardRegistrationRequestParameterNames.Jwks)]
        public string Jwks { get; set; }

        [DataMember(Name = Core.Constants.StandardRegistrationRequestParameterNames.SectoreIdentifierUri)]
        public string SectorIdentifierUri { get; set; }

        [DataMember(Name = Core.Constants.StandardRegistrationRequestParameterNames.SubjectType)]
        public string SubjectType { get; set; }

        [DataMember(Name = Core.Constants.StandardRegistrationRequestParameterNames.IdTokenSignedResponseAlg)]
        public string IdTokenSignedResponseAlg { get; set; }

        [DataMember(Name = Core.Constants.StandardRegistrationRequestParameterNames.IdTokenEncryptedResponseAlg)]
        public string IdTokenEncryptedResponseAlg { get; set; }

        [DataMember(Name = Core.Constants.StandardRegistrationRequestParameterNames.IdTokenEncryptedResponseEnc)]
        public string IdTokenEncryptedResponseEnc { get; set; }

        [DataMember(Name = Core.Constants.StandardRegistrationRequestParameterNames.UserInfoSignedResponseAlg)]
        public string UserInfoSignedResponseAlg { get; set; }

        [DataMember(Name = Core.Constants.StandardRegistrationRequestParameterNames.UserInfoEncryptedResponseAlg)]
        public string UserInfoEncryptedResponseAlg { get; set; }

        [DataMember(Name = Core.Constants.StandardRegistrationRequestParameterNames.UserInfoEncryptedResponseEnc)]
        public string UserInfoEncryptedResponseEnc { get; set; }

        [DataMember(Name = Core.Constants.StandardRegistrationRequestParameterNames.RequestObjectSigningAlg)]
        public string RequestObjectSigningAlg { get; set; }

        [DataMember(Name = Core.Constants.StandardRegistrationRequestParameterNames.RequestObjectEncryptionAlg)]
        public string RequestObjectEncryptionAlg { get; set; }

        [DataMember(Name = Core.Constants.StandardRegistrationRequestParameterNames.RequestObjectEncryptionEnc)]
        public string RequestObjectEncryptionEnc { get; set; }

        [DataMember(Name = Core.Constants.StandardRegistrationRequestParameterNames.TokenEndPointAuthMethod)]
        public string TokenEndPointAuthMethod { get; set; }

        [DataMember(Name = Core.Constants.StandardRegistrationRequestParameterNames.TokenEndPointAuthSigningAlg)]
        public string TokenEndPointAuthSigningAlg { get; set; }

        [DataMember(Name = Core.Constants.StandardRegistrationRequestParameterNames.DefaultMaxAge)]
        public string DefaultMaxAge { get; set; }

        [DataMember(Name = Core.Constants.StandardRegistrationRequestParameterNames.RequireAuthTime)]
        public string RequireAuthTime { get; set; }

        [DataMember(Name = Core.Constants.StandardRegistrationRequestParameterNames.DefaultAcrValues)]
        public string DefaultAcrValues { get; set; }

        [DataMember(Name = Core.Constants.StandardRegistrationRequestParameterNames.InitiateLoginUri)]
        public string InitiateLoginUri { get; set; }

        [DataMember(Name = Core.Constants.StandardRegistrationRequestParameterNames.RequestUris)]
        public List<string> RequestUris { get; set; }
    }
}