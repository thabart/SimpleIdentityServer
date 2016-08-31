using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Elasticsearch.Net;
using Serilog.Events;
using Serilog.Sinks.PeriodicBatching;

namespace Serilog.Sinks.Elasticsearch
{
    /// <summary>
    /// Writes log events as documents to ElasticSearch.
    /// </summary>
    public class ElasticsearchSink : PeriodicBatchingSink
    {

        private readonly ElasticsearchSinkState _state;

        /// <summary>
        /// Creates a new ElasticsearchSink instance with the provided options
        /// </summary>
        /// <param name="options">Options configuring how the sink behaves, may NOT be null</param>
        public ElasticsearchSink(ElasticsearchSinkOptions options)
            : base(options.BatchPostingLimit, options.Period)
        {
            _state = ElasticsearchSinkState.Create(options);
            _state.RegisterTemplateIfNeeded();
        }

        /// <summary>
        /// Emit a batch of log events, running to completion synchronously.
        /// </summary>
        /// <param name="events">The events to emit.</param>
        /// <remarks>
        /// Override either <see cref="M:Serilog.Sinks.PeriodicBatching.PeriodicBatchingSink.EmitBatch(System.Collections.Generic.IEnumerable{Serilog.Events.LogEvent})" />
        ///  or <see cref="M:Serilog.Sinks.PeriodicBatching.PeriodicBatchingSink.EmitBatchAsync(System.Collections.Generic.IEnumerable{Serilog.Events.LogEvent})" />,
        /// not both.
        /// </remarks>
        protected override void EmitBatch(IEnumerable<LogEvent> events)
        {
            this.EmitBatchChecked<DynamicResponse>(events);
        }

        /// <summary>
        /// Emit a batch of log events, running to completion synchronously.
        /// </summary>
        /// <param name="events">The events to emit.</param>
        /// <returns>Response from Elasticsearch</returns>
        protected virtual ElasticsearchResponse<T> EmitBatchChecked<T>(IEnumerable<LogEvent> events) where T : class
        {
            // ReSharper disable PossibleMultipleEnumeration
            if (events == null || !events.Any())
                return null;

            var payload = new List<string>();
            foreach (var e in events)
            {
                var indexName = _state.GetIndexForEvent(e, e.Timestamp.ToUniversalTime());
                var action = new { index = new { _index = indexName, _type = _state.Options.TypeName } };
                var actionJson = _state.Serialize(action);
                payload.Add(actionJson);
                var sw = new StringWriter();
                _state.Formatter.Format(e, sw);
                payload.Add(sw.ToString());
            }
            return _state.Client.Bulk<T>(payload);
        }
    }
}