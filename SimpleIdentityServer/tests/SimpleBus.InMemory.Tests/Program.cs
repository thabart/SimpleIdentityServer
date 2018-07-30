using SimpleBus.Core;
using SimpleIdentityServer.OAuth.Events;
using System;
using System.Collections.Generic;

namespace SimpleBus.InMemory.Tests
{
    public static class Program
    {
        private static InMemoryEventPublisher _publisher;
        private static InMemoryEventSubscriber _subscriber;

        public static void Main(string[] args)
        {
            Console.WriteLine("INMEMORY");
            IEventHandler handler =  new TokenGrantedHandler();
            var handlers = new List<IEventHandler>
            {
                handler
            };
            var options = new InMemoryOptions();
            _publisher = new InMemoryEventPublisher(options); // Publish the events
            _subscriber = new InMemoryEventSubscriber(options, handlers); // Subscribe the events
            _subscriber.Listen();
            while(true)
            {
                Console.WriteLine("Type a key to send a message message");
                Console.ReadLine();
                var evt = new TokenGranted("id", "processId", "payload", 1);
                _publisher.Publish(evt);
            }
        }
    }
}