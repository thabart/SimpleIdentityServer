using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace SimpleIdentityServer.AccountFilter.Basic.Common
{
    [DataContract]
    public enum ComparisonOperationsDto
    {
        [EnumMember(Value = Constants.ComparisonOperationsDtoNames.Equal)]
        Equal,
        [EnumMember(Value = Constants.ComparisonOperationsDtoNames.NotEqual)]
        NotEqual,
        [EnumMember(Value = Constants.ComparisonOperationsDtoNames.RegularExpression)]
        RegularExpression
    }
}
