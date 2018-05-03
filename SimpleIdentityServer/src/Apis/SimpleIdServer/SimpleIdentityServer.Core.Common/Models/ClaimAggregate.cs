using System;

namespace SimpleIdentityServer.Core.Common.Models
{
    public class ClaimAggregate
    {
        public string Code { get; set; }
        public bool IsIdentifier { get; set; }
        public DateTime CreateDateTime { get; set; }
        public DateTime UpdateDateTime { get; set; }
    }
}
