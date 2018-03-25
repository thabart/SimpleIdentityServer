using SimpleBus.Core;

namespace SimpleIdentityServer.Scim.Handler.Events
{
    public class ScimErrorReceived : Event
    {
        public ScimErrorReceived(string id, string processId, string message, int order)
        {
            Id = id;
            ProcessId = processId;
            Message = message;
            Order = order;
        }

        public string Id { get; private set; }
        public string ProcessId { get; private set; }
        public string Message { get; private set; }
        public int Order { get; private set; }
    }
}
