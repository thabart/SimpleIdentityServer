using System.Runtime.Serialization;

namespace SimpleIdentityServer.Authenticate.SMS.Common.Requests
{
    [DataContract]
    public class ConfirmationCodeRequest
    {
        [DataMember(Name = "phone_number")]
        public string PhoneNumber { get; set; }
    }
}