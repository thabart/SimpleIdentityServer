using System;

namespace SimpleIdentityServer.Core.Common.Models
{
    public class ClaimAggregate
    {
        public ClaimAggregate() { }

        public ClaimAggregate(string code, string value)
        {
            Code = code;
            Value = value;
        }

        public string Code { get; set; }
        public string Value { get; set; }
        public bool IsIdentifier { get; set; }
        public DateTime CreateDateTime { get; set; }
        public DateTime UpdateDateTime { get; set; }
    }
}
