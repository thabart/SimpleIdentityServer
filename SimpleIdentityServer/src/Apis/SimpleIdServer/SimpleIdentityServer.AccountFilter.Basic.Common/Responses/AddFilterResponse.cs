using System.Runtime.Serialization;

namespace SimpleIdentityServer.AccountFilter.Basic.Common.Responses
{
    [DataContract]
    public class AddFilterResponse
    {
        [DataMember(Name = Constants.FilterResponseNames.Id)]
        public string Id { get; set; }
    }
}
