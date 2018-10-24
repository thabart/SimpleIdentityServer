using System.Runtime.Serialization;

namespace SimpleIdentityServer.Common.Dtos.Responses
{
    [DataContract]
    public class ErrorResponse
    {
        [DataMember(Name = Constants.ErrorResponseNames.Error)]
        public string Error { get; set; }
        [DataMember(Name = Constants.ErrorResponseNames.ErrorDescription)]
        public string ErrorDescription { get; set; }
        [DataMember(Name = Constants.ErrorResponseNames.ErrorUri)]
        public string ErrorUri { get; set; }
    }
}
