using System;
using System.Runtime.Serialization;

namespace SimpleIdentityServer.Manager.Common.Responses
{
    [DataContract]
    public class ClaimResponse
    {
        [DataMember(Name = Constants.ClaimResponseNames.Code)]
        public string Code { get; set; }

        [DataMember(Name = Constants.ClaimResponseNames.IsIdentifier)]
        public bool IsIdentifier  { get; set; }

        [DataMember(Name = Constants.ClaimResponseNames.CreateDateTime)]
        public DateTime CreateDateTime { get; set; }

        [DataMember(Name = Constants.ClaimResponseNames.UpdateDateTime)]
        public DateTime UpdateDateTime { get; set; }
    }
}
