using Microsoft.AspNetCore.SignalR.Client;
using SimpleBus.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleBus.InMemory
{
    public sealed class InMemoryEventSubscriber : IEventSubscriber, IDisposable
    {
        private SignalrConnection _instance;
        private HubConnection _connection; 
        private readonly InMemoryOptions _options;
        private readonly IEnumerable<IEventHandler> _eventHandlers;

        public InMemoryEventSubscriber(InMemoryOptions options, IEnumerable<IEventHandler> eventHandlers)
        {
            _options = options;
            _eventHandlers = eventHandlers;
        }

        public void Listen()
        {
            _instance = SignalrConnection.Instance(_options);
            _instance.Connected += HandleConnected;
            _instance.Connect();
        }

        private void HandleConnected(object sender, EventArgs e)
        {
            _instance.GetHubConnection().On("Event", new[] { typeof(string) }, (parameters, message) =>
            {
                var msg = Convert.FromBase64String(parameters.First().ToString());
                var json = Encoding.UTF8.GetString(msg);
                ProcessMessageHelper.Process(json, _eventHandlers);
                return Task.FromResult(0);
            }, null);
        }

        public void Dispose()
        {
            if (_instance != null)
            {
                if(_instance.IsConnected)
                {
                    _instance.GetHubConnection().StopAsync().Wait();
                }
            }
        }
    }
}
