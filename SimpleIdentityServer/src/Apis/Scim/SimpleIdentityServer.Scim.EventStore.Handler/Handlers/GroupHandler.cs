using SimpleBus.Core;
using SimpleIdentityServer.EventStore.Core.Models;
using SimpleIdentityServer.EventStore.Core.Repositories;
using SimpleIdentityServer.Scim.Handler.Events;
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Scim.EventStore.Handler.Handlers
{
    public class GroupHandler : IHandle<AddGroupReceived>, IHandle<AddGroupFinished>, IHandle<UpdateGroupReceived>
        , IHandle<UpdateGroupFinished>, IHandle<RemoveGroupReceived>, IHandle<RemoveGroupFinished>
        , IHandle<PatchGroupReceived>, IHandle<PatchGroupFinished>
    {
        private readonly IEventAggregateRepository _repository;

        public GroupHandler(IEventAggregateRepository repository)
        {
            _repository = repository;
        }

        public async Task Handle(AddGroupReceived parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            await AddEvent(parameter.Id, parameter.ProcessId, parameter.Payload, "Start add group", parameter.Order, "add_group_started");
        }

        public async Task Handle(AddGroupFinished parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            await AddEvent(parameter.Id, parameter.ProcessId, parameter.Payload, "Finish add group", parameter.Order, "add_group_finished");
        }

        public async Task Handle(UpdateGroupReceived parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            await AddEvent(parameter.Id, parameter.ProcessId, parameter.Payload, "Start to update the group", parameter.Order, "update_group_started");
        }

        public async Task Handle(UpdateGroupFinished parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            await AddEvent(parameter.Id, parameter.ProcessId, parameter.Payload, "Finish to update the group", parameter.Order, "update_group_finished");
        }

        public async Task Handle(RemoveGroupReceived parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            await AddEvent(parameter.Id, parameter.ProcessId, parameter.Payload, "Start to remove group", parameter.Order, "remove_group_started");
        }

        public async Task Handle(RemoveGroupFinished parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            await AddEvent(parameter.Id, parameter.ProcessId, parameter.Payload, "Finish to remove group", parameter.Order, "remove_group_finished");
        }

        public async Task Handle(PatchGroupReceived parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            await AddEvent(parameter.Id, parameter.ProcessId, parameter.Payload, "Start to patch group", parameter.Order, "patch_group_started");
        }

        public async Task Handle(PatchGroupFinished parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            await AddEvent(parameter.Id, parameter.ProcessId, parameter.Payload, "Finish to patch group", parameter.Order, "patch_group_finished");
        }

        private async Task AddEvent(string id, string processId, string content, string message, int order, string key)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (string.IsNullOrWhiteSpace(processId))
            {
                throw new ArgumentNullException(nameof(processId));
            }

            await _repository.Add(new EventAggregate
            {
                Id = id,
                CreatedOn = DateTime.UtcNow,
                Description = message,
                AggregateId = processId,
                Payload = content,
                Order = order,
                Key = key,
                Type = Constants.Type,
                Verbosity = EventVerbosities.Information
            });
        }
    }
}
