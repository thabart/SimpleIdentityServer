using System;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.RollingFile;

namespace Serilog.Sinks.Elasticsearch
{
    class DurableElasticsearchSink : ILogEventSink, IDisposable
    {
        // we rely on the date in the filename later!
        const string FileNameSuffix = "-{Date}.json";

        readonly RollingFileSink _sink;
        readonly ElasticsearchLogShipper _shipper;
        readonly ElasticsearchSinkState _state;

        public DurableElasticsearchSink(ElasticsearchSinkOptions options)
        {
            _state = ElasticsearchSinkState.Create(options);

            if (string.IsNullOrWhiteSpace(options.BufferBaseFilename))
            {
                throw new ArgumentException("Cannot create the durable ElasticSearch sink without a buffer base file name!");
            }

            _sink = new RollingFileSink(
                options.BufferBaseFilename + FileNameSuffix,
                _state.DurableFormatter,
                options.BufferFileSizeLimitBytes,
                null);

            _shipper = new ElasticsearchLogShipper(_state);
        }

        public void Emit(LogEvent logEvent)
        {
            _sink.Emit(logEvent);
        }

        public void Dispose()
        {
            _sink.Dispose();
            _shipper.Dispose();
        }
    }
}