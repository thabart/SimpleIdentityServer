using SimpleBus.Core;

namespace SimpleIdentityServer.Scim.Handler.Events
{
    public class RemoveUserReceived : Event
    {
        public RemoveUserReceived(string id, string processId, string payload, int order)
        {
            Id = id;
            ProcessId = processId;
            Payload = payload;
            Order = order;
        }

        public string Id { get; private set; }
        public string ProcessId { get; private set; }
        public string Payload { get; private set; }
        public int Order { get; private set; }
    }
}
