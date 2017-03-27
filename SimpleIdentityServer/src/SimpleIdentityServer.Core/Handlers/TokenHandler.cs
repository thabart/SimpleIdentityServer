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

using Newtonsoft.Json;
using SimpleIdentityServer.Core.Bus;
using SimpleIdentityServer.Core.Events;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Repositories;
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Core.Handlers
{
    public class TokenHandler : IHandle<GrantTokenViaAuthorizationCodeReceived>, IHandle<GrantTokenViaClientCredentialsReceived>, IHandle<GrantTokenViaRefreshTokenReceived>, IHandle<GrantTokenViaResourceOwnerCredentialsReceived>, IHandle<RevokeTokenReceived>, IHandle<TokenGranted>, IHandle<TokenRevoked>
    {
        private readonly IEventAggregateRepository _repository;

        public TokenHandler(IEventAggregateRepository repository)
        {
            _repository = repository;
        }

        public async Task Handle(GrantTokenViaAuthorizationCodeReceived message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            await Add(message.Id, message.ProcessId, message.Parameter, "Start grant token via authorization code", message.Order);
        }

        public async Task Handle(GrantTokenViaClientCredentialsReceived message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            await Add(message.Id, message.ProcessId, message.Parameter, "Start grant token via client credentials", message.Order);
        }

        public async Task Handle(GrantTokenViaRefreshTokenReceived message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            await Add(message.Id, message.ProcessId, message.Parameter, "Start grant token via refresh token", message.Order);
        }

        public async Task Handle(GrantTokenViaResourceOwnerCredentialsReceived message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            await Add(message.Id, message.ProcessId, message.Parameter, "Start grant token via resource owner credentials", message.Order);
        }

        public async Task Handle(RevokeTokenReceived message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            await Add(message.Id, message.ProcessId, message.Parameter, "Start revoke token", message.Order);
        }

        public async Task Handle(TokenGranted message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            await Add(message.Id, message.ProcessId, message.Parameter, "Finish grant token", message.Order);
        }

        public async Task Handle(TokenRevoked message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            await Add(message.Id, message.ProcessId, "Finish revoke token", message.Order);
        }

        private Task Add<T>(string id, string processId, T content, string message, int order)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            var payload = JsonConvert.SerializeObject(content);
            return Add(id, processId, payload, message, order);
        }

        private Task Add(string id, string processId, string message, int order)
        {
            return Add(id, processId, null, message, order);
        }

        private async Task Add(string id, string processId, string payload, string message, int order)
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
                AggregateId = processId,
                Description = message,
                CreatedOn = DateTime.UtcNow,
                Payload = payload,
                Order = order
            });
        }
    }
}
