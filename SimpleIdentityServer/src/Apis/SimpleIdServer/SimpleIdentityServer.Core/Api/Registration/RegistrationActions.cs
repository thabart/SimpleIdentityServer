﻿#region copyright
// Copyright 2015 Habart Thierry
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

using SimpleIdServer.Bus;
using SimpleIdentityServer.Core.Api.Registration.Actions;
using SimpleIdentityServer.Core.Common.DTOs.Responses;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.OAuth.Events;
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Core.Api.Registration
{
    public interface IRegistrationActions
    {
        Task<ClientRegistrationResponse> PostRegistration(RegistrationParameter registrationParameter);
    }

    public class RegistrationActions : IRegistrationActions
    {
        private readonly IRegisterClientAction _registerClientAction;
        private readonly IEventPublisher _eventPublisher;
        private readonly IPayloadSerializer _payloadSerializer;

        public RegistrationActions(IRegisterClientAction registerClientAction, IEventPublisher eventPublisher,
            IPayloadSerializer payloadSerializer)
        {
            _registerClientAction = registerClientAction;
            _eventPublisher = eventPublisher;
            _payloadSerializer = payloadSerializer;
        }

        public async Task<ClientRegistrationResponse> PostRegistration(RegistrationParameter registrationParameter)
        {
            if (registrationParameter == null)
            {
                throw new ArgumentNullException(nameof(registrationParameter));
            }

            var processId = Guid.NewGuid().ToString();
            try
            {
                _eventPublisher.Publish(new RegistrationReceived(Guid.NewGuid().ToString(), processId, _payloadSerializer.GetPayload(registrationParameter), 0));
                var result = await _registerClientAction.Execute(registrationParameter);
                _eventPublisher.Publish(new RegistrationResultReceived(Guid.NewGuid().ToString(), processId, _payloadSerializer.GetPayload(result), 1));
                return result;
            }
            catch(IdentityServerException ex)
            {
                _eventPublisher.Publish(new OAuthErrorReceived(Guid.NewGuid().ToString(), processId, ex.Code, ex.Message, 1));
                throw;
            }
        }
    }
}
