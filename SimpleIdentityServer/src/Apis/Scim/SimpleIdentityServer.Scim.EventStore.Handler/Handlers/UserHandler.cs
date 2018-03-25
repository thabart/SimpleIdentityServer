using SimpleBus.Core;
using SimpleIdentityServer.EventStore.Core.Models;
using SimpleIdentityServer.EventStore.Core.Repositories;
using SimpleIdentityServer.Scim.Handler.Events;
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Scim.EventStore.Handler.Handlers
{
    public class UserHandler : IHandle<AddUserReceived>, IHandle<AddUserFinished>, IHandle<UpdateUserReceived>
        , IHandle<UpdateUserFinished>, IHandle<RemoveUserReceived>, IHandle<RemoveUserFinished>
        , IHandle<PatchUserReceived>, IHandle<PatchUserFinished>
    {
        private readonly IEventAggregateRepository _repository;

        public UserHandler(IEventAggregateRepository repository)
        {
            _repository = repository;
        }

        public async Task Handle(AddUserReceived parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            await AddEvent(parameter.Id, parameter.ProcessId, parameter.Payload, "Start add user", parameter.Order, "add_user_started");
        }

        public async Task Handle(AddUserFinished parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            await AddEvent(parameter.Id, parameter.ProcessId, parameter.Payload, "Finish add user", parameter.Order, "add_user_finished");
        }

        public async Task Handle(UpdateUserReceived parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            await AddEvent(parameter.Id, parameter.ProcessId, parameter.Payload, "Start to update the user", parameter.Order, "update_user_started");
        }

        public async Task Handle(UpdateUserFinished parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            await AddEvent(parameter.Id, parameter.ProcessId, parameter.Payload, "Finish to update the user", parameter.Order, "update_user_finished");
        }

        public async Task Handle(RemoveUserReceived parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            await AddEvent(parameter.Id, parameter.ProcessId, parameter.Payload, "Start to remove user", parameter.Order, "remove_user_started");
        }

        public async Task Handle(RemoveUserFinished parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            await AddEvent(parameter.Id, parameter.ProcessId, parameter.Payload, "Finish to remove user", parameter.Order, "remove_user_finished");
        }

        public async Task Handle(PatchUserReceived parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            await AddEvent(parameter.Id, parameter.ProcessId, parameter.Payload, "Start to patch user", parameter.Order, "patch_user_started");
        }

        public async Task Handle(PatchUserFinished parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            await AddEvent(parameter.Id, parameter.ProcessId, parameter.Payload, "Finish to patch user", parameter.Order, "patch_user_finished");
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
