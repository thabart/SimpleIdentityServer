using System.Collections.Generic;

namespace SimpleIdentityServer.Uma.Core.Models
{
    public class TicketLine
    {
        public string Id { get; set; }
        public IEnumerable<string> Scopes { get; set; }
        public string ResourceSetId { get; set; }
    }
}
