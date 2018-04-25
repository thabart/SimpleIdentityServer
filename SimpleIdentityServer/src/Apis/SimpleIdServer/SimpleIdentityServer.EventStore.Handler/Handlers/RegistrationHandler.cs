#region copyright
// Copyright 2017 Habart Thierry
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using SimpleBus.Core;
using SimpleIdentityServer.EventStore.Core.Models;
using SimpleIdentityServer.EventStore.Core.Repositories;
using SimpleIdentityServer.Handler.Events;
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.EventStore.Handler.Handlers
{
    public class RegistrationHandler : IHandle<RegistrationReceived>, IHandle<RegistrationResultReceived>
    {
        private readonly IEventAggregateRepository _repository;
        private readonly EventStoreHandlerOptions _options;

        public RegistrationHandler(IEventAggregateRepository repository, EventStoreHandlerOptions options)
        {
            _repository = repository;
            _options = options;
        }

        public async Task Handle(RegistrationReceived message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            await _repository.Add(new EventAggregate
            {
                Id = message.Id,
                AggregateId = message.ProcessId,
                Description = "Start client registration",
                CreatedOn = DateTime.UtcNow,
                Payload = message.Payload,
                Order = message.Order,
                Type = _options.Type,
                Verbosity = EventVerbosities.Information,
                Key = "register_client_started"
            });
        }

        public async Task Handle(RegistrationResultReceived message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }
            
            await _repository.Add(new EventAggregate
            {
                Id = message.Id,
                AggregateId = message.ProcessId,
                Description = "Client registered",
                CreatedOn = DateTime.UtcNow,
                Payload = message.Payload,
                Order = message.Order,
                Type = _options.Type,
                Verbosity = EventVerbosities.Information,
                Key = "register_client_finished"
            });
        }
    }
}
