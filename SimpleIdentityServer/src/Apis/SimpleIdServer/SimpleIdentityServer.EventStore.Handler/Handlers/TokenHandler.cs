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
    public class TokenHandler : IHandle<GrantTokenViaAuthorizationCodeReceived>, IHandle<GrantTokenViaClientCredentialsReceived>, IHandle<GrantTokenViaRefreshTokenReceived>, IHandle<GrantTokenViaResourceOwnerCredentialsReceived>, IHandle<RevokeTokenReceived>, IHandle<TokenGranted>, IHandle<TokenRevoked>
    {
        private readonly IEventAggregateRepository _repository;
        private readonly EventStoreHandlerOptions _options;

        public TokenHandler(IEventAggregateRepository repository, EventStoreHandlerOptions options)
        {
            _repository = repository;
            _options = options;
        }

        public async Task Handle(GrantTokenViaAuthorizationCodeReceived message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }
            
            await Add(message.Id, message.ProcessId, message.Payload, "Start grant token via authorization code", message.Order, "auth_code_grantype_started");
        }

        public async Task Handle(GrantTokenViaClientCredentialsReceived message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }
            
            await Add(message.Id, message.ProcessId, message.Payload, "Start grant token via client credentials", message.Order, "client_credentials_grantype_started");
        }

        public async Task Handle(GrantTokenViaRefreshTokenReceived message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }
            
            await Add(message.Id, message.ProcessId, message.Payload, "Start grant token via refresh token", message.Order, "refresh_token_grantype_started");
        }

        public async Task Handle(GrantTokenViaResourceOwnerCredentialsReceived message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }
            
            await Add(message.Id, message.ProcessId, message.Payload, "Start grant token via resource owner credentials", message.Order, "resource_owner_grantype_started");
        }

        public async Task Handle(RevokeTokenReceived message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }
            
            await Add(message.Id, message.ProcessId, message.Payload, "Start revoke token", message.Order, "revoke_token_started");
        }

        public async Task Handle(TokenGranted message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }
            
            await Add(message.Id, message.ProcessId, message.Payload, "Token granted", message.Order, "token_granted");
        }

        public async Task Handle(TokenRevoked message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            await Add(message.Id, message.ProcessId, "Token revoked", message.Order, "token_revoked");
        }

        private Task Add(string id, string processId, string message, int order, string key)
        {
            return Add(id, processId, null, message, order, key);
        }

        private async Task Add(string id, string processId, string payload, string message, int order, string key)
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
                Order = order,
                Key = key,
                Type = _options.Type,
                Verbosity = EventVerbosities.Information
            });
        }
    }
}
