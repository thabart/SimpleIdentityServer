using System.Collections.Generic;

namespace SimpleIdentityServer.Uma.Core.Policies
{
    public class TicketLineParameter
    {
        public TicketLineParameter(string clientId, IEnumerable<string> scopes = null, bool isAuthorizedByRo = false)
        {
            ClientId = clientId;
            Scopes = scopes;
            IsAuthorizedByRo = isAuthorizedByRo;
        }

        public string ClientId { get; set; }
        public IEnumerable<string> Scopes { get; set; }
        public bool IsAuthorizedByRo { get; set; }
    }
}
