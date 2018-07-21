﻿#region copyright
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

using Newtonsoft.Json;
using SimpleIdentityServer.Core.Bus;
using SimpleIdentityServer.Core.Events;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Repositories;
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Core.Handlers
{
    public class IntrospectionHandler : IHandle<IntrospectionRequestReceived>, IHandle<IntrospectionResultReturned>
    {
        private readonly IEventAggregateRepository _repository;
        private readonly IPayloadSerializer _payloadSerializer;

        public IntrospectionHandler(IEventAggregateRepository repository, IPayloadSerializer payloadSerializer)
        {
            _repository = repository;
            _payloadSerializer = payloadSerializer;
        }

        public async Task Handle(IntrospectionRequestReceived message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            var payload = _payloadSerializer.GetPayload(message);
            await _repository.Add(new EventAggregate
            {
                Id = message.Id,
                AggregateId = message.ProcessId,
                Description = "Start introspection",
                CreatedOn = DateTime.UtcNow,
                Payload = payload,
                Order = message.Order
            }).ConfigureAwait(false);
        }

        public async Task Handle(IntrospectionResultReturned message)
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
                Description = "Introspection result",
                CreatedOn = DateTime.UtcNow,
                Payload = payload,
                Order = message.Order
            }).ConfigureAwait(false);
        }
    }
}
