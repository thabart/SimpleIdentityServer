namespace SimpleIdentityServer.EventStore.Handler
{
    public enum ServerTypes
    {
        AUTH,
        OPENID
    }

    public class EventStoreHandlerOptions
    {
        public EventStoreHandlerOptions(ServerTypes type)
        {
            switch(type)
            {
                case ServerTypes.AUTH:
                    Type = "auth";
                    break;
                case ServerTypes.OPENID:
                    Type = "openid";
                    break;
            }
        }

        public string Type { get; private set; }
    }
}
