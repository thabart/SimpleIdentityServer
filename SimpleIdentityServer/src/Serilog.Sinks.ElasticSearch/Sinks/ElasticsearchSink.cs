using Elasticsearch.Net;
using Serilog.Events;
using Serilog.Formatting.Json;
using Serilog.Sinks.PeriodicBatching;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Serilog.Sinks.ElasticSearch.Sinks
{
    public class ElasticsearchSink : PeriodicBatchingSink
    {
        #region Fields

        private readonly ElasticsearchSinkOptions _options;

        private readonly JsonFormatter _formatter;

        private readonly ElasticLowLevelClient _client;

        #endregion

        #region Constructor

        public ElasticsearchSink(ElasticsearchSinkOptions options) 
            : base(options.BatchPostingLimit, options.Period)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _options = options;
            _formatter = new ElasticsearchJsonFormatter();
            _client = GetClient(options.Url);
        }

        #endregion

        #region Public methods

        protected override void EmitBatch(IEnumerable<LogEvent> events)
        {
            if (events == null ||
                !events.Any())
            {
                return;
            }

            var payload = new List<string>();
            foreach(var e in events)
            {
                var indexName = GetIndexName(e.Timestamp.ToUniversalTime());
                var action = new { index = new { _index = indexName, _type = _options.TypeName } };
                var actionJson = _client.Serializer.SerializeToString(action, SerializationFormatting.None);
                payload.Add(actionJson);
                var sw = new StringWriter();
                _formatter.Format(e, sw);
                payload.Add(sw.ToString());
            }

            _client.Bulk<DynamicResponse>(payload);
        }

        #endregion

        #region Private methods

        private string GetIndexName(DateTimeOffset entryDateTime)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}-{1:yyyy.MM.dd}", _options.IndexName, entryDateTime);
        }

        #endregion

        #region Private static methods

        private static ElasticLowLevelClient GetClient(string url)
        {
            var node = new Uri(url);
            var config = new ConnectionConfiguration(node);
            return new ElasticLowLevelClient(config);
        }

        #endregion
    }
}
