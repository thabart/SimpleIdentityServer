using Microsoft.AspNetCore.SignalR.Client;

namespace SimpleBus.InMemory
{
    internal class SignalrConnection
    {
        private static SignalrConnection _instance;
        private HubConnection _connection;

        private SignalrConnection(InMemoryOptions options)
        {
            _connection = new HubConnectionBuilder().WithUrl(options.Url).Build();
            _connection.StartAsync().Wait();
        }

        public static SignalrConnection Instance(InMemoryOptions options)
        {
            if (_instance == null)
            {
                _instance = new SignalrConnection(options);
            }

            return _instance;
        }

        public HubConnection GetHubConnection()
        {
            return _connection;
        }
    }
}
