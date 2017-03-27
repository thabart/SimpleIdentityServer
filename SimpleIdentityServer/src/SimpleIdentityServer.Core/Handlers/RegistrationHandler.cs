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

using System;
using System.Threading.Tasks;
using SimpleIdentityServer.Core.Bus;
using SimpleIdentityServer.Core.Events;
using SimpleIdentityServer.Core.Repositories;
using Newtonsoft.Json;
using SimpleIdentityServer.Core.Models;

namespace SimpleIdentityServer.Core.Handlers
{
    public class RegistrationHandler : IHandle<RegistrationReceived>, IHandle<RegistrationResultReceived>
    {
        private readonly IEventAggregateRepository _repository;

        public RegistrationHandler(IEventAggregateRepository repository)
        {
            _repository = repository;
        }

        public async Task Handle(RegistrationResultReceived message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            var payload = JsonConvert.SerializeObject(message.Parameter);
            await _repository.Add(new EventAggregate
            {
                Id = message.Id,
                AggregateId = message.ProcessId,
                Description = "Finish client registration",
                CreatedOn = DateTime.UtcNow,
                Payload = payload,
                Order = message.Order
            });
        }

        public async Task Handle(RegistrationReceived message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            var payload = JsonConvert.SerializeObject(message.Parameter);
            await _repository.Add(new EventAggregate
            {
                Id = message.Id,
                AggregateId = message.ProcessId,
                Description = "Start client registration",
                CreatedOn = DateTime.UtcNow,
                Payload = payload,
                Order = message.Order
            });
        }
    }
}
