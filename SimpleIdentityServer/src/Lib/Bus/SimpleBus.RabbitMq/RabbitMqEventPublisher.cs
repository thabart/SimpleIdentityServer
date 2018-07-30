using Newtonsoft.Json;
using RabbitMQ.Client;
using SimpleBus.Core;
using System.Text;

namespace SimpleBus.RabbitMq
{
    internal sealed class RabbitMqEventPublisher : IEventPublisher
    {
        private readonly RabbitMqOptions _options;

        public RabbitMqEventPublisher(RabbitMqOptions options)
        {
            _options = options;
        }

        public void Publish<T>(T evt) where T : Event
        {
            var factory = new ConnectionFactory
            {
                HostName = _options.HostName
            };
            if (!string.IsNullOrWhiteSpace(_options.UserName))
            {
                factory.UserName = _options.UserName;
            }

            if (!string.IsNullOrWhiteSpace(_options.Password))
            {
                factory.Password = _options.Password;
            }
            
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.ExchangeDeclare(exchange: _options.BrokerName, type: "fanout");
                    evt.ServerName = _options.ServerName;
                    var serializedMessage = new SerializedMessage
                    {
                        AssemblyQualifiedName = typeof(T).AssemblyQualifiedName,
                        Content = JsonConvert.SerializeObject(evt)
                    };
                    var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(serializedMessage));
                    channel.BasicPublish(exchange: _options.BrokerName,
                        routingKey: "",
                        basicProperties: null,
                        body: body);
                }
            }           
        }
    }
}
