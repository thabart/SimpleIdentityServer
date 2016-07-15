using Serilog.Configuration;
using Serilog.Events;
using Serilog.Sinks.ElasticSearch.Sinks;

namespace Serilog.Sinks.ElasticSearch
{
    public static class LoggerConfigurationElasticsearchExtensions
    {
        #region Public static methods

        public static LoggerConfiguration Elasticsearch(
            this LoggerSinkConfiguration loggerSinkConfiguration,
            ElasticsearchSinkOptions options = null)
        {
            options = options ?? new ElasticsearchSinkOptions();
            var sink = new ElasticsearchSink(options);
            return loggerSinkConfiguration.Sink(sink, options.MinimumLogEventLevel ?? LevelAlias.Minimum);
        }

        #endregion
    }
}
