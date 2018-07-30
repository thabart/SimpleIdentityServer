using RabbitMQ.Client;
using SimpleBus.Core;

namespace SimpleBus.RabbitMq
{
    public class RabbitMqOptions : SimpleBusOptions
    {
        public RabbitMqOptions()
        {
            HostName = "localhost";
            BrokerName = "sid_events";
            Port = AmqpTcpEndpoint.DefaultAmqpSslPort;
        }

        public string HostName { get; set; }
        public string BrokerName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public int Port { get; set; }
    }
}
