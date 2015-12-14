using System;

namespace SimpleIdentityServer.Logging.Consumer
{
    class Program
    {
        private const string LoggingEventSourceName = "SimpleIdentityServer";
        private const string EtwConfigurationFileName = "SemanticLogging.xml";

        static void Main(string[] args)
        {
            // THE URL TO ACCESS TO THE EVENT INFORMATION IS SOMETHING LIKE : http://localhost:9200/slab-2015.12.14/SimpleIdentityServer/AVGhMgwSc8OeJI7Ykror
            // USE OUT-PROCESS
            using (var service = new TraceEventServiceHost())
            {
                service.Start();
                Console.WriteLine();
                Console.ReadLine();
            }

            // 

            // USE IN-PROCESS
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
