using System.Runtime.Serialization;

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
    }
}
