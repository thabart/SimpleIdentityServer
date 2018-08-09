using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleBus.InMemory
{
    internal class SignalrConnection
    {
        private static SignalrConnection _instance;
        private InMemoryOptions _options;
        private HubConnection _connection;

        private SignalrConnection(InMemoryOptions options)
        {
            _options = options;
            Connect();
        }

        public event EventHandler Connected;
        public event EventHandler Disconnected;

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

        private Task HandleClosed(Exception arg)
        {
            IsConnected = false;
            if (Disconnected != null)
            {
                Disconnected(this, EventArgs.Empty);
            }

            Retry();
            return Task.FromResult(0);
        }

        private void Connect()
        {
            try
            {
                _connection = new HubConnectionBuilder().WithUrl(_options.Url).Build();
                _connection.StartAsync().Wait();
                _connection.Closed += HandleClosed;
                IsConnected = true;
                if(Connected != null)
                {
                    Connected(this, EventArgs.Empty);
                }
            }
            catch(Exception)
            {
                Retry();
            }
        }

        private void Retry()
        {
            if (!_options.IsRetryEnabled)
            {
                return;
            }

            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(_options.RetryTimestampInMs);
                Connect();
            });
        }
    }
}