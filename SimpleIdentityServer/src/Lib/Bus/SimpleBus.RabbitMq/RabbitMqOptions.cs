using SimpleBus.Core;

namespace SimpleBus.RabbitMq
{
    public class RabbitMqOptions : SimpleBusOptions
    {
        public RabbitMqOptions()
        {
            ConnectionString = "localhost";
            BrokerName = "sid_events";
        }

        public string ConnectionString { get; set; }
        public string BrokerName { get; set; }
    }
}
