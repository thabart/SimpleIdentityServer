using System.Runtime.Serialization;

namespace SimpleIdentityServer.Manager.Host.DTOs.Requests
{
    [DataContract]
    public sealed class GetJwsRequest
    {
        [DataMember(Name = Constants.GetJwsRequestNames.Jws)]
        public string Jws { get; set; }

        [DataMember(Name = Constants.GetJwsRequestNames.Url)]
        public string Url { get; set; }
    }
}
