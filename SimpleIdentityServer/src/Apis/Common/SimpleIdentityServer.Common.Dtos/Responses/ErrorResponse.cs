using System.Runtime.Serialization;

namespace SimpleIdentityServer.Common.Dtos.Responses
{
    [DataContract]
    public class ErrorResponse
    {
        [DataMember(Name = Constants.ErrorResponseNames.Code)]
        public string Code { get; set; }
        [DataMember(Name = Constants.ErrorResponseNames.Message)]
        public string Message { get; set; }
    }
}
