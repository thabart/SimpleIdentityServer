using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SimpleBus.Core;
using System.Collections.Generic;
using System.Text;

namespace SimpleBus.RabbitMq
{
    public sealed class RabbitMqEventSubscriber : IEventSubscriber
    {
        private readonly RabbitMqOptions _options;
        private IConnection _connection;
        private IEnumerable<IEventHandler> _eventHandlers;

        public RabbitMqEventSubscriber(RabbitMqOptions options, IEnumerable<IEventHandler> eventHandlers) 
        {
            _options = options;
            _eventHandlers = eventHandlers;
        }

        public void Listen()
        {
            var factory = new ConnectionFactory
            {
                HostName = _options.HostName
            };
            _connection = factory.CreateConnection();
            var channel = _connection.CreateModel();
            channel.ExchangeDeclare(exchange: _options.BrokerName, type: "fanout");
            var queueName = channel.QueueDeclare().QueueName;
            channel.QueueBind(queue: queueName, exchange: _options.BrokerName, routingKey: "");
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += HandleMessageReceived;
            channel.BasicConsume(queue: queueName,
                                 autoAck: true,
                                 consumer: consumer);
        }

        private void HandleMessageReceived(object sender, BasicDeliverEventArgs e)
        {
            if (_eventHandlers == null)
            {
                return;
            }

            var json = Encoding.UTF8.GetString(e.Body);
            ProcessMessageHelper.Process(json, _eventHandlers);
        }
    }
}