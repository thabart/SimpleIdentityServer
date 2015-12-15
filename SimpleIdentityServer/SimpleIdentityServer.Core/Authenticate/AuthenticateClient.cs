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

using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Validators;

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

        public AuthenticateClient(
            IClientSecretBasicAuthentication clientSecretBasicAuthentication,
            IClientSecretPostAuthentication clientSecretPostAuthentication,
            IClientAssertionAuthentication clientAssertionAuthentication,
            IClientValidator clientValidator)
        {
            _clientSecretBasicAuthentication = clientSecretBasicAuthentication;
            _clientSecretPostAuthentication = clientSecretPostAuthentication;
            _clientAssertionAuthentication = clientAssertionAuthentication;
            _clientValidator = clientValidator;
        }

        public Client Authenticate(
            AuthenticateInstruction instruction,
            out string errorMessage)
        {
            errorMessage = string.Empty;
            Client client = null;
            // First we try to get the client_id
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
            switch (tokenEndPointAuthMethod)
            {
                case TokenEndPointAuthenticationMethods.client_secret_basic:
                    client = _clientSecretBasicAuthentication.AuthenticateClient(instruction, client);
                    break;
                case TokenEndPointAuthenticationMethods.client_secret_post:
                    client = _clientSecretPostAuthentication.AuthenticateClient(instruction, client);
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
