using System;

namespace SimpleIdentityServer.EventStore.Core.Models
{
    public enum EventVerbosities
    {
        Information,
        Error
    }

    public class EventAggregate
    {
        public string Id { get; set; }
        public string AggregateId { get; set; }
        public string Payload { get; set; }
        public string Description { get; set; }
        public int Order { get; set; }
        public EventVerbosities Verbosity { get; set; }
        public string Type { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
