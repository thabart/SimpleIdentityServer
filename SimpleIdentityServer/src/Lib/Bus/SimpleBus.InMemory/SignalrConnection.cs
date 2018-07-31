using Microsoft.AspNetCore.SignalR.Client;
using System.Threading.Tasks;

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
            _connection.Closed += HandleClosed;
            IsConnected = true;
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

        public bool IsConnected { get; private set; }

        private Task HandleClosed(System.Exception arg)
        {
            IsConnected = false;
            return Task.FromResult(0);
        }
    }
}
