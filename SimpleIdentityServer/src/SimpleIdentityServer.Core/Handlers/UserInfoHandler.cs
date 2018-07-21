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
using Newtonsoft.Json;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Core.Models;

namespace SimpleIdentityServer.Core.Handlers
{
    public class UserInfoHandler : IHandle<GetUserInformationReceived>, IHandle<UserInformationReturned>
    {
        private readonly IEventAggregateRepository _repository;
        private readonly IPayloadSerializer _serializer;

        public UserInfoHandler(IEventAggregateRepository repository, IPayloadSerializer serializer)
        {
            _repository = repository;
            _serializer = serializer;
        }

        public async Task Handle(GetUserInformationReceived parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            var payload = _serializer.GetPayload(parameter);
            await AddEvent(parameter.Id, parameter.ProcessId, payload, "Start user information", parameter.Order).ConfigureAwait(false);
        }

        public async Task Handle(UserInformationReturned parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            var payload = _serializer.GetPayload(parameter);
            await AddEvent(parameter.Id, parameter.ProcessId, payload, "User information returned", parameter.Order).ConfigureAwait(false);
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
            }).ConfigureAwait(false);
        }
    }
}
