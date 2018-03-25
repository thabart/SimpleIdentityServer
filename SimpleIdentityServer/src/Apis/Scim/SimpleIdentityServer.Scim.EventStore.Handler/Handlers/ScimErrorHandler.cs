using Newtonsoft.Json.Linq;
using SimpleBus.Core;
using SimpleIdentityServer.EventStore.Core.Models;
using SimpleIdentityServer.EventStore.Core.Repositories;
using SimpleIdentityServer.Scim.Handler.Events;
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Scim.EventStore.Handler.Handlers
{
    public class ScimErrorHandler : IHandle<ScimErrorReceived>
    {
        private readonly IEventAggregateRepository _repository;

        public ScimErrorHandler(IEventAggregateRepository repository)
        {
            _repository = repository;
        }

        public async Task Handle(ScimErrorReceived message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            var jObj = new JObject();
            jObj.Add("error", message.Message);
            await _repository.Add(new EventAggregate
            {
                Id = message.Id,
                AggregateId = message.ProcessId,
                CreatedOn = DateTime.UtcNow,
                Description = "An error occured",
                Payload = jObj.ToString(),
                Order = message.Order,
                Type = Constants.Type,
                Verbosity = EventVerbosities.Error,
                Key = "error"
            });
        }
    }
}
