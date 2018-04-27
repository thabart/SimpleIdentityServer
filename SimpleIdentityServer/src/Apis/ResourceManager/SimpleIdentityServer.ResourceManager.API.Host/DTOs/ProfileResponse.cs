using System.Runtime.Serialization;

namespace SimpleIdentityServer.ResourceManager.API.Host.DTOs
{
    [DataContract]
    public class ProfileResponse
    {
        [DataMember(Name = Constants.ProfileResponseNames.OpenidUrl)]
        public string OpenidUrl { get; set; }
        [DataMember(Name = Constants.ProfileResponseNames.AuthUrl)]
        public string AuthUrl { get; set; }
        [DataMember(Name = Constants.ProfileResponseNames.ScimUrl)]
        public string ScimUrl { get; set; }
    }
}
