using Elasticsearch.Net;
using Serilog.Events;
using Serilog.Formatting.Json;
using Serilog.Parsing;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace Serilog.Sinks.ElasticSearch
{
    public class ElasticsearchJsonFormatter : JsonFormatter
    {
        #region Fields

        private readonly IElasticsearchSerializer _serializer;

        private readonly bool _inlineFields;

        #endregion

        #region Constructor

        public ElasticsearchJsonFormatter(bool omitEnclosingObject = false,
            string closingDelimiter = null,
            bool renderMessage = false,
            IFormatProvider formatProvider = null,
            IElasticsearchSerializer serializer = null,
            bool inlineFields = false)
            : base(omitEnclosingObject, closingDelimiter, renderMessage, formatProvider)
        {
            _serializer = serializer;
            _inlineFields = inlineFields;
        }

        #endregion

        #region Protected methods

        protected override void WriteRenderings(
            IGrouping<string, PropertyToken>[] tokensWithFormat, 
            IReadOnlyDictionary<string, LogEventPropertyValue> properties, 
            TextWriter output)
        {
            output.Write(",\"{0}\":{{", "renderings");
            WriteRenderingsValues(tokensWithFormat, properties, output);
            output.Write("}");
        }
        
        protected override void WriteProperties(
            IReadOnlyDictionary<string, LogEventPropertyValue> properties,
            TextWriter output)
        {
            if (!_inlineFields)
                output.Write(",\"{0}\":{{", "fields");
            else
                output.Write(",");

            WritePropertiesValues(properties, output);

            if (!_inlineFields)
                output.Write("}");
        }

        protected override void WriteDictionary(
            IReadOnlyDictionary<ScalarValue, LogEventPropertyValue> elements, 
            TextWriter output)
        {
            var escaped = elements.ToDictionary(e => DotEscapeFieldName(e.Key), e => e.Value);

            base.WriteDictionary(escaped, output);
        }

        protected override void WriteJsonProperty(
            string name, 
            object value, 
            ref string precedingDelimiter, 
            TextWriter output)
        {
            name = DotEscapeFieldName(name);

            base.WriteJsonProperty(name, value, ref precedingDelimiter, output);
        }

        protected virtual ScalarValue DotEscapeFieldName(ScalarValue value)
        {
            var s = value.Value as string;
            return s != null ? new ScalarValue(DotEscapeFieldName(s)) : value;
        }

        protected virtual string DotEscapeFieldName(string value)
        {
            if (value == null) return null;

            return value.Replace('.', '/');
        }

        protected override void WriteException(
            Exception exception, 
            ref string delim, 
            TextWriter output)
        {
            output.Write(delim);
            output.Write("\"");
            output.Write("exceptions");
            output.Write("\":[");

            delim = "";
            WriteExceptionSerializationInfo(exception, ref delim, output, depth: 0);
            output.Write("]");
        }

        protected void WriteSingleException(Exception exception, ref string delim, TextWriter output, int depth)
        {
            var helpUrl = exception.HelpLink;
            var stackTrace = exception.StackTrace;
            var hresult = exception.HResult;
            var source = exception.Source;

            //TODO Loop over ISerializable data


            WriteJsonProperty("Depth", depth, ref delim, output);
            WriteJsonProperty("Message", exception.Message, ref delim, output);
            WriteJsonProperty("Source", source, ref delim, output);
            WriteJsonProperty("StackTraceString", stackTrace, ref delim, output);
            WriteJsonProperty("HResult", hresult, ref delim, output);
            WriteJsonProperty("HelpURL", helpUrl, ref delim, output);

            //writing byte[] will fall back to serializer and they differ in output 
            //JsonNET assumes string, simplejson writes array of numerics.
            //Skip for now
            //this.WriteJsonProperty("WatsonBuckets", watsonBuckets, ref delim, output);

        }
        
        protected override void WriteRenderedMessage(string message, ref string delim, TextWriter output)
        {
            WriteJsonProperty("message", message, ref delim, output);
        }
        
        protected override void WriteMessageTemplate(string template, ref string delim, TextWriter output)
        {
            WriteJsonProperty("messageTemplate", template, ref delim, output);
        }

        protected override void WriteLevel(LogEventLevel level, ref string delim, TextWriter output)
        {
            var stringLevel = Enum.GetName(typeof(LogEventLevel), level);
            WriteJsonProperty("level", stringLevel, ref delim, output);
        }
        
        protected override void WriteTimestamp(DateTimeOffset timestamp, ref string delim, TextWriter output)
        {
            WriteJsonProperty("@timestamp", timestamp, ref delim, output);
        }

        protected override void WriteLiteralValue(object value, TextWriter output)
        {
            if (_serializer != null)
            {
                string jsonString = _serializer.SerializeToString(value, SerializationFormatting.None);
                output.Write(jsonString);
                return;
            }

            base.WriteLiteralValue(value, output);
        }

        #endregion

        #region Private methods

        private void WriteExceptionSerializationInfo(
            Exception exception, 
            ref string delim, 
            TextWriter output, 
            int depth)
        {
            output.Write(delim);
            output.Write("{");
            delim = "";
            WriteSingleException(exception, ref delim, output, depth);
            output.Write("}");

            delim = ",";
            if (exception.InnerException != null && depth < 20)
                this.WriteExceptionSerializationInfo(exception.InnerException, ref delim, output, ++depth);
        }

        private void WriteStructuredExceptionMethod(string exceptionMethodString, ref string delim, TextWriter output)
        {
            if (string.IsNullOrWhiteSpace(exceptionMethodString)) return;

            var args = exceptionMethodString.Split('\0', '\n');

            if (args.Length != 5) return;

            var memberType = Int32.Parse(args[0], CultureInfo.InvariantCulture);
            var name = args[1];
            var assemblyName = args[2];
            var className = args[3];
            var signature = args[4];
            var an = new AssemblyName(assemblyName);
            output.Write(delim);
            output.Write("\"");
            output.Write("ExceptionMethod");
            output.Write("\":{");
            delim = "";
            this.WriteJsonProperty("Name", name, ref delim, output);
            this.WriteJsonProperty("AssemblyName", an.Name, ref delim, output);
            this.WriteJsonProperty("AssemblyVersion", an.Version.ToString(), ref delim, output);
            this.WriteJsonProperty("AssemblyCulture", an.CultureName, ref delim, output);
            this.WriteJsonProperty("ClassName", className, ref delim, output);
            this.WriteJsonProperty("Signature", signature, ref delim, output);
            this.WriteJsonProperty("MemberType", memberType, ref delim, output);

            output.Write("}");
            delim = ",";
        }

        #endregion
    }
}
