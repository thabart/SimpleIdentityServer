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
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Validators;
using SimpleIdentityServer.Logging;

namespace SimpleIdentityServer.Core.Authenticate
{
    public interface IAuthenticateClient
    {
        Client Authenticate(
            AuthenticateInstruction instruction,
            out string errorMessage);
    }

    public class AuthenticateClient : IAuthenticateClient
    {
        private readonly IClientSecretBasicAuthentication _clientSecretBasicAuthentication;

        private readonly IClientSecretPostAuthentication _clientSecretPostAuthentication;

        private readonly IClientAssertionAuthentication _clientAssertionAuthentication;

        private readonly IClientValidator _clientValidator;

        private readonly ISimpleIdentityServerEventSource _simpleIdentityServerEventSource;

        public AuthenticateClient(
            IClientSecretBasicAuthentication clientSecretBasicAuthentication,
            IClientSecretPostAuthentication clientSecretPostAuthentication,
            IClientAssertionAuthentication clientAssertionAuthentication,
            IClientValidator clientValidator,
            ISimpleIdentityServerEventSource simpleIdentityServerEventSource)
        {
            _clientSecretBasicAuthentication = clientSecretBasicAuthentication;
            _clientSecretPostAuthentication = clientSecretPostAuthentication;
            _clientAssertionAuthentication = clientAssertionAuthentication;
            _clientValidator = clientValidator;
            _simpleIdentityServerEventSource = simpleIdentityServerEventSource;
        }

        public Client Authenticate(
            AuthenticateInstruction instruction,
            out string errorMessage)
        {
            if (instruction == null)
            {
                throw new ArgumentNullException("the authentication instruction cannot be null");    
            }

            errorMessage = string.Empty;
            Client client = null;

            // First we try to fetch the client_id
            // The different client authentication mechanisms are described here : http://openid.net/specs/openid-connect-core-1_0.html#ClientAuthentication
            var clientId = TryGettingClientId(instruction);
            if (!string.IsNullOrWhiteSpace(clientId))
            {
                client = _clientValidator.ValidateClientExist(clientId);
            }

            if (client == null)
            {
                errorMessage = ErrorDescriptions.TheClientCannotBeAuthenticated;
                return null;
            }
            
            var tokenEndPointAuthMethod = client.TokenEndPointAuthMethod;
            var authenticationType = Enum.GetName(typeof (TokenEndPointAuthenticationMethods),
                tokenEndPointAuthMethod);
            _simpleIdentityServerEventSource.StartToAuthenticateTheClient(client.ClientId,
                authenticationType);
            switch (tokenEndPointAuthMethod)
            {
                case TokenEndPointAuthenticationMethods.client_secret_basic:
                    client = _clientSecretBasicAuthentication.AuthenticateClient(instruction, client);
                    if (client == null)
                    {
                        errorMessage = ErrorDescriptions.TheClientCannotBeAuthenticatedWithSecretBasic;
                    }
                    break;
                case TokenEndPointAuthenticationMethods.client_secret_post:
                    client = _clientSecretPostAuthentication.AuthenticateClient(instruction, client);
                    if (client == null)
                    {
                        errorMessage = ErrorDescriptions.TheClientCannotBeAuthenticatedWithSecretPost;
                    }
                    break;
                case TokenEndPointAuthenticationMethods.client_secret_jwt:
                    client = _clientAssertionAuthentication.AuthenticateClientWithClientSecretJwt(instruction,
                        client.ClientSecret, out errorMessage);
                    break;
                case TokenEndPointAuthenticationMethods.private_key_jwt:
                    client = _clientAssertionAuthentication.AuthenticateClientWithPrivateKeyJwt(instruction,
                        out errorMessage);
                    break;
            }

            if (client != null)
            {
                _simpleIdentityServerEventSource.FinishToAuthenticateTheClient(client.ClientId,
                    authenticationType);
            }

            return client;
        }

        /// <summary>
        /// Try to get the client id from the HTTP body or HTTP header.
        /// </summary>
        /// <param name="instruction">Authentication instruction</param>
        /// <returns>Client id</returns>
        private string TryGettingClientId(AuthenticateInstruction instruction)
        {
            var clientId = _clientAssertionAuthentication.GetClientId(instruction);
            if (!string.IsNullOrWhiteSpace(clientId))
            {
                return clientId;
            }

            clientId= _clientSecretBasicAuthentication.GetClientId(instruction);
            if (!string.IsNullOrWhiteSpace(clientId))
            {
                return clientId;
            }

            return _clientSecretPostAuthentication.GetClientId(instruction);
        }
    }
}
