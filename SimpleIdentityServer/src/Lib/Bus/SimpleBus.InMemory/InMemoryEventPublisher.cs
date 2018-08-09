using Newtonsoft.Json;
using SimpleBus.Core;
using System;
using System.Text;

namespace SimpleBus.InMemory
{
    public class InMemoryEventPublisher : IEventPublisher
    {
        private readonly InMemoryOptions _options;
        private readonly SignalrConnection _instance;

        public InMemoryEventPublisher(InMemoryOptions options)
        {
            _options = options;
            _instance = SignalrConnection.Instance(options);
            _instance.Connect();
        }

        public void Publish<T>(T evt) where T : Event
        {
            if (evt == null)
            {
                throw new ArgumentNullException(nameof(evt));
            }

            evt.ServerName = _options.ServerName;
            var hubConnection = _instance.GetHubConnection();
            var serializedMessage = new SerializedMessage
            {
                AssemblyQualifiedName = typeof(T).AssemblyQualifiedName,
                Content = JsonConvert.SerializeObject(evt)
            };
            var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(serializedMessage));
            if (SignalrConnection.Instance(_options).IsConnected)
            {
                hubConnection.InvokeCoreAsync("BroadCastMessages", typeof(string), new[] { body }).Wait();
            }
        }
    }
}
