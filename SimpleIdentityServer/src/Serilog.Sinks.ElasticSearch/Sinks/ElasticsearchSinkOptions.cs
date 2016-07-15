using Serilog.Events;
using System;

namespace Serilog.Sinks.ElasticSearch.Sinks
{
    public class ElasticsearchSinkOptions
    {
        #region Constructor

        public ElasticsearchSinkOptions()
        {
            BatchPostingLimit = 50;
            Period = TimeSpan.FromSeconds(2);
            IndexName = "simpleidserver";
            TypeName = "log";
            Url = "http://localhost:9200";
        }

        #endregion

        #region Properties

        public int BatchPostingLimit { get; set; }

        public TimeSpan Period { get; set; }

        public LogEventLevel? MinimumLogEventLevel { get; set; }

        public string IndexName { get; set; }

        public string TypeName { get; set; }

        public string Url { get; set; }

        #endregion
    }
}
