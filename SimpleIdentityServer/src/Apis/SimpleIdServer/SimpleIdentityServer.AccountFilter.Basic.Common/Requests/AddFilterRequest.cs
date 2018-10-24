using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SimpleIdentityServer.AccountFilter.Basic.Common.Requests
{
    [DataContract]
    public class AddFilterRuleRequest
    {
        [DataMember(Name = Constants.FilterRuleResponseNames.ClaimKey)]
        public string ClaimKey { get; set; }
        [DataMember(Name = Constants.FilterRuleResponseNames.ClaimValue)]
        public string ClaimValue { get; set; }
        [DataMember(Name = Constants.FilterRuleResponseNames.Operation)]
        [JsonConverter(typeof(StringEnumConverter))]
        public ComparisonOperationsDto Operation { get; set; }
    }

    [DataContract]
    public class AddFilterRequest
    {
        [DataMember(Name = Constants.FilterResponseNames.Name)]
        public string Name { get; set; }
        [DataMember(Name = Constants.FilterResponseNames.Rules)]
        public IEnumerable<AddFilterRuleRequest> Rules { get; set; }
    }
}
