using System.Runtime.Serialization;
using SimpleIdentityServer.Common.Dtos.Responses;

namespace SimpleIdentityServer.Core.Common.DTOs.Responses
{
    [DataContract]
    public class ErrorResponseWithState : ErrorResponse
    {
        [DataMember(Name = ErrorResponseWithStateNames.State)]
        public string State { get; set; }
    }
}
