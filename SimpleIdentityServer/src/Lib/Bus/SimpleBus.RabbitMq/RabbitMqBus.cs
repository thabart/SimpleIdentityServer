using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SimpleBus.Core;
using SimpleBus.RabbitMq.Exceptions;
using System;
using System.Text;
using System.Threading.Tasks;

namespace SimpleBus.RabbitMq
{
    public class RabbitMqBus : IEventPublisher, IDisposable
    {
        private readonly IEvtHandlerStore _evtHandlerStore;
        private const string _brokerName = "simplebus_event_bus";
        private const string _connectionString = "localhost";
        private IConnection _connection;
        private IModel _channel;
        private string _queueName;
        private IModel _consumeChannel;

        public RabbitMqBus(IEvtHandlerStore evtHandlerStore)
        {
            _evtHandlerStore = evtHandlerStore;
        }

        public void Init()
        {
            var factory = new ConnectionFactory() { HostName = _connectionString };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _consumeChannel = CreateConsumerChannel();
        }

        public void Publish<T>(T evt) where T : Event
        {
            if (_connection == null || !_connection.IsOpen)
            {
                throw new ConnectionNotOpenedException();
            }

            var eventName = typeof(T).Name;
            using (var channel = _connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: _brokerName, type: "direct");
                string message = JsonConvert.SerializeObject(evt);
                var body = Encoding.UTF8.GetBytes(message);
                channel.BasicPublish(exchange: _brokerName,
                    routingKey: eventName,
                    basicProperties: null,
                    body: body);
            }
        }

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

        private async Task ProcessEvent(string eventName, string message)
        {

        }

        public void Dispose()
        {
            _consumeChannel.Dispose();
            _channel.Dispose();
            _connection.Dispose();
        }
    }
}
