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

using SimpleIdentityServer.EventStore.Core.Models;
using SimpleIdentityServer.EventStore.Core.Repositories;
using SimpleIdentityServer.Handler.Bus;
using SimpleIdentityServer.Handler.Events;
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Core.Handlers
{
    public class UserInfoHandler : IHandle<GetUserInformationReceived>, IHandle<UserInformationReturned>
    {
        private readonly IEventAggregateRepository _repository;

        public UserInfoHandler(IEventAggregateRepository repository)
        {
            _repository = repository;
        }

        public async Task Handle(GetUserInformationReceived parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }
            
            await AddEvent(parameter.Id, parameter.ProcessId, parameter.Payload, "Start user information", parameter.Order);
        }

        public async Task Handle(UserInformationReturned parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }
            
            await AddEvent(parameter.Id, parameter.ProcessId, parameter.Payload, "User information returned", parameter.Order);
        }

        private async Task AddEvent(string id, string processId, string content, string message, int order)
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
                Order = order
            });
        }
    }
}
