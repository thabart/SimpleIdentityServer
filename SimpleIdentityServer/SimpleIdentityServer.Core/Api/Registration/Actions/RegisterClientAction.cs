#region copyright
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

using System;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Validators;
using SimpleIdentityServer.Logging;
using System.Linq;
using System.Collections.Generic;

namespace SimpleIdentityServer.Core.Api.Registration.Actions
{
    public interface IRegisterClientAction
    {
        Client Execute(RegistrationParameter registrationParameter);
    }

    public class RegisterClientAction : IRegisterClientAction
    {
        private readonly IRegistrationParameterValidator _registrationParameterValidator;

        private readonly ISimpleIdentityServerEventSource _simpleIdentityServerEventSource;

        public RegisterClientAction(
            IRegistrationParameterValidator registrationParameterValidator,
            ISimpleIdentityServerEventSource simpleIdentityServerEventSource)
        {
            _registrationParameterValidator = registrationParameterValidator;
            _simpleIdentityServerEventSource = simpleIdentityServerEventSource;
        }

        public Client Execute(RegistrationParameter registrationParameter)
        {
            if (registrationParameter == null)
            {
                throw new ArgumentNullException("registrationParameter");
            }

            // Validate the parameters
            _registrationParameterValidator.Validate(registrationParameter);

            _simpleIdentityServerEventSource.StartRegistration(registrationParameter.ClientName);

            // Generate the client
            var client = new Client();
            client.RedirectionUrls = registrationParameter.RedirectUris;

            // If omitted then the default value is authorization code response type
            if (registrationParameter.ResponseTypes == null ||
                !registrationParameter.ResponseTypes.Any())
            {
                client.ResponseTypes = new List<ResponseType>
                {
                    ResponseType.code
                };
            }
            else
            {
                client.ResponseTypes = registrationParameter.ResponseTypes;
            }

            // If omitted then the default value is authorization code grant type
            if (registrationParameter.GrantTypes == null ||
                !registrationParameter.GrantTypes.Any())
            {
                client.GrantTypes = new List<GrantType>
                {
                    GrantType.authorization_code
                };
            } else
            {
                client.GrantTypes = registrationParameter.GrantTypes;
            }

            client.ApplicationType = registrationParameter.ApplicationType == null ? ApplicationTypes.web
                : registrationParameter.ApplicationType.Value;

            return client;
        }
    }
}
