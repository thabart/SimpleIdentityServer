using System;

namespace SimpleIdentityServer.Logging.Consumer
{
    class Program
    {
        private const string LoggingEventSourceName = "SimpleIdentityServer";
        private const string EtwConfigurationFileName = "SemanticLogging.xml";

        static void Main(string[] args)
        {
            // IT'S IN PROCESS
            /*
            var eventListener = new ObservableEventListener();

            eventListener.EnableEvents(
                SimpleIdentityServerEventSource.Log,
                EventLevel.LogAlways,
                Keywords.All);

            // Log to the console application
            eventListener.LogToConsole();

            // Log to a flat file
            var formatter = new EventTextFormatter
            {
                DateTimeFormat = "yyyy-dd-MM"
            };

            eventListener.LogToFlatFile("log.txt", formatter);
            */

            Console.ReadLine();
        }
    }
}
