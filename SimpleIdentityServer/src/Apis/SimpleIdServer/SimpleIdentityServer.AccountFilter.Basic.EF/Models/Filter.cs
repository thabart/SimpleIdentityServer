using System;
using System.Collections.Generic;

namespace SimpleIdentityServer.AccountFilter.Basic.EF.Models
{
    public class Filter
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public DateTime CreateDateTime { get; set; }
        public DateTime UpdateDateTime { get; set; }
        public virtual ICollection<FilterRule> Rules { get; set; }
    }
}
