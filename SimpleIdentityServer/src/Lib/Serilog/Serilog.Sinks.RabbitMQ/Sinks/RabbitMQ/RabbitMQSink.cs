using System;
using System.IO;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Raw;
using Serilog.Sinks.RabbitMQ.Sinks.RabbitMQ;
using Serilog.Sinks.PeriodicBatching;
using System.Collections.Generic;

namespace Serilog.Sinks.RabbitMQ
{
    /// <summary>
    /// Serilog RabbitMq Sink - Lets you log to RabbitMq using Serilog
    /// </summary>
    public class RabbitMQSink : PeriodicBatchingSink
    {
        private readonly ITextFormatter _formatter;
        private readonly IFormatProvider _formatProvider;
        private readonly RabbitMQClient _client;

        public RabbitMQSink(RabbitMQConfiguration configuration,
            ITextFormatter formatter,
            IFormatProvider formatProvider) : base(configuration.BatchPostingLimit, configuration.Period)
        {
            _formatter = formatter ?? new RawFormatter();
            _formatProvider = formatProvider;
            _client = new RabbitMQClient(configuration);
        }

        protected override void EmitBatch(IEnumerable<LogEvent> events)
        {
            foreach (var logEvent in events)
            {
                var sw = new StringWriter();
                _formatter.Format(logEvent, sw);
                _client.Publish(sw.ToString());
            }
        }
    }
}