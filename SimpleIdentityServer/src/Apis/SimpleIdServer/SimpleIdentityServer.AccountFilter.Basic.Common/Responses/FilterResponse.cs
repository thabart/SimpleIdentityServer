using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SimpleIdentityServer.AccountFilter.Basic.Common.Responses
{
    [DataContract]
    public class FilterRuleResponse
    {
        [DataMember(Name = Constants.FilterRuleResponseNames.Id)]
        public string Id { get; set; }
        [DataMember(Name = Constants.FilterRuleResponseNames.ClaimKey)]
        public string ClaimKey { get; set; }
        [DataMember(Name = Constants.FilterRuleResponseNames.ClaimValue)]
        public string ClaimValue { get; set; }
        [DataMember(Name = Constants.FilterRuleResponseNames.Operation)]
        [JsonConverter(typeof(StringEnumConverter))]
        public ComparisonOperationsDto Operation { get; set; }
    }

    [DataContract]
    public class FilterResponse
    {
        [DataMember(Name = Constants.FilterResponseNames.Id)]
        public string Id { get; set; }
        [DataMember(Name = Constants.FilterResponseNames.CreateDateTime)]
        public DateTime CreateDateTime { get; set; }
        [DataMember(Name = Constants.FilterResponseNames.UpdateDateTime)]
        public DateTime UpdateDateTime { get; set; }
        [DataMember(Name = Constants.FilterResponseNames.Name)]
        public string Name { get; set; }
        [DataMember(Name = Constants.FilterResponseNames.Rules)]
        public IEnumerable<FilterRuleResponse> Rules { get; set; }
    }
}
