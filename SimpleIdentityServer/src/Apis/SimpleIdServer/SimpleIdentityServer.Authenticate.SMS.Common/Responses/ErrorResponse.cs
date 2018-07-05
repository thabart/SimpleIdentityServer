using System.Runtime.Serialization;

namespace SimpleIdentityServer.Authenticate.SMS.Common.Responses
{
    public class ErrorResponse
    {
        [DataMember(Name = "code")]
        public string Code { get; set; }
        [DataMember(Name = "message")]
        public string Message { get; set; }
    }
}
