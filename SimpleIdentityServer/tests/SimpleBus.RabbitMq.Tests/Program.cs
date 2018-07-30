using SimpleBus.Core;
using SimpleIdentityServer.OAuth.Events;
using System;
using System.Collections.Generic;

namespace SimpleBus.RabbitMq.Tests
{
    public static class Program
    {
        private static RabbitMqEventPublisher _publisher;
        private static RabbitMqEventSubscriber _subscriber;

        public static void Main(string[] args)
        {
            Console.WriteLine("RABBMITMQ");
            IEventHandler handler =  new TokenGrantedHandler();
            var handlers = new List<IEventHandler>
            {
                handler
            };
            var options = new RabbitMqOptions();
            _publisher = new RabbitMqEventPublisher(options); // Publish the events
            _subscriber = new RabbitMqEventSubscriber(options, handlers); // Subscribe the events
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