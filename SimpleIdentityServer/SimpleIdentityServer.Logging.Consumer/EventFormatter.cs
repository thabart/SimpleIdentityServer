using Microsoft.Practices.EnterpriseLibrary.SemanticLogging;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging.Formatters;

namespace SimpleIdentityServer.Logging.Consumer
{
    public sealed class EventFormatter : IEventTextFormatter
    {
        public void WriteEvent(EventEntry eventEntry, System.IO.TextWriter writer)
        {
            // Check this link to have a custom event formatter ;)
            // https://msdn.microsoft.com/en-us/library/dn775004(v=pandp.20).aspx
        }
    }
}
