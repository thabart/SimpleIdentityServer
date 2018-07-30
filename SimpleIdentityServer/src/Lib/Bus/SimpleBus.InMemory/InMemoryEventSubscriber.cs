using SimpleBus.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleBus.InMemory
{
    public sealed class InMemoryEventSubscriber : IEventSubscriber
    {
        private readonly InMemoryOptions _options;
        private readonly IEnumerable<IEventHandler> _eventHandlers;

        public InMemoryEventSubscriber(InMemoryOptions options, IEnumerable<IEventHandler> eventHandlers)
        {
            _options = options;
            _eventHandlers = eventHandlers;
        }

        public void Listen()
        {
            var connection = SignalrConnection.Instance(_options).GetHubConnection();
            connection.On("Event", new[] { typeof(string) }, (parameters, message) =>
            {
                var msg = Convert.FromBase64String(parameters.First().ToString());
                var json = Encoding.UTF8.GetString(msg);
                ProcessMessageHelper.Process(json, _eventHandlers);
                return Task.FromResult(0);
            }, null);
        }
    }
}
