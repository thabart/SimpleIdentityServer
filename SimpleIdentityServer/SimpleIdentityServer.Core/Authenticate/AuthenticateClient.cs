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
            if (instruction.ClientAssertionType == Constants.StandardClientAssertionTypes.JwtBearer)
            {
                return _clientAssertionAuthentication.AuthenticateClient(instruction, out errorMessage);
            }

            Client client = null;
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

            client = ValidateClientCredentials(client, instruction);
            if (client == null)
            {
                errorMessage = ErrorDescriptions.TheClientCannotBeAuthenticated;
            }

            return client;
        }

        private Client ValidateClientCredentials(
            Client client,
            AuthenticateInstruction authenticateInstruction)
        {
            Client result = null;
            switch (client.TokenEndPointAuthMethod)
            {
                case TokenEndPointAuthenticationMethods.client_secret_post:
                    result = _clientSecretPostAuthentication.AuthenticateClient(authenticateInstruction, client);
                    break;
                case TokenEndPointAuthenticationMethods.client_secret_basic:
                    result = _clientSecretBasicAuthentication.AuthenticateClient(authenticateInstruction, client);
                    break;
            }

            return result;
        }

        /// <summary>
        /// Try to get the client id from the HTTP body or HTTP header.
        /// </summary>
        /// <param name="instruction">Authentication instruction</param>
        /// <returns>Client id</returns>
        private string TryGettingClientId(AuthenticateInstruction instruction)
        {
            var clientId= _clientSecretBasicAuthentication.GetClientId(instruction);
            if (!string.IsNullOrWhiteSpace(clientId))
            {
                return clientId;
            }

            return _clientSecretPostAuthentication.GetClientId(instruction);
        }
    }
}
