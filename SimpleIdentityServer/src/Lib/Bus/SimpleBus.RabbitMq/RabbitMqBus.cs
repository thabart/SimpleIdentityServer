using Newtonsoft.Json;
using RabbitMQ.Client;
using SimpleBus.Core;
using System.Text;

namespace SimpleBus.RabbitMq
{
    internal sealed class RabbitMqBus : IEventPublisher
    {
        private readonly RabbitMqOptions _options;

        public RabbitMqBus(RabbitMqOptions options)
        {
            _options = options;
        }

        public void Publish<T>(T evt) where T : Event
        {
            var factory = new ConnectionFactory
            {
                HostName = _options.ConnectionString
            };
            using (var connection = factory.CreateConnection())
            {
                var eventName = typeof(T).Name;
                using (var channel = connection.CreateModel())
                {
                    channel.ExchangeDeclare(exchange: _options.BrokerName, type: "direct");
                    evt.ServerName = _options.ServerName;
                    string message = JsonConvert.SerializeObject(evt);
                    var body = Encoding.UTF8.GetBytes(message);
                    channel.BasicPublish(exchange: _options.BrokerName,
                        routingKey: eventName,
                        basicProperties: null,
                        body: body);
                }
            }           
        }

        /*

        private IModel CreateConsumerChannel()
        {
            var channel = _connection.CreateModel();
            channel.ExchangeDeclare(exchange: _brokerName, type: "direct");
            _queueName = channel.QueueDeclare().QueueName;
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                var eventName = ea.RoutingKey;
                var message = Encoding.UTF8.GetString(ea.Body);
            };
            channel.BasicConsume(queue: _queueName,
                                 autoAck: false,
                                 consumer: consumer);
            return channel;
        }
        */
    }
}
