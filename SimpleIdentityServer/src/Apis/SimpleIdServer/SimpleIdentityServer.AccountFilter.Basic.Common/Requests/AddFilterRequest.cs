using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SimpleIdentityServer.AccountFilter.Basic.Common.Requests
{
    [DataContract]
    public class AddFilterRuleRequest
    {
        [DataMember(Name = Constants.AddFilterRuleRequestNames.ClaimKey)]
        public string ClaimKey { get; set; }
        [DataMember(Name = Constants.AddFilterRuleRequestNames.ClaimValue)]
        public string ClaimValue { get; set; }
    }

    [DataContract]
    public class AddFilterRequest
    {
        [DataMember(Name = Constants.AddFilterRequestNames.Name)]
        public string Name { get; set; }
        [DataMember(Name = Constants.AddFilterRequestNames.Rules)]
        public IEnumerable<AddFilterRuleRequest> Rules { get; set; }
    }
}
