namespace SimpleIdentityServer.Core.Authenticate
{
    public class AuthenticateClient
    {
        private readonly IClientSecretBasicAuthentication _clientSecretBasicAuthentication;

        private readonly IClientSecretPostAuthentication _clientSecretPostAuthentication;

        private readonly IClientAssertionAuthentication _clientAssertionAuthentication;

        public AuthenticateClient(
            IClientSecretBasicAuthentication clientSecretBasicAuthentication,
            IClientSecretPostAuthentication clientSecretPostAuthentication,
            IClientAssertionAuthentication clientAssertionAuthentication)
        {
            _clientSecretBasicAuthentication = clientSecretBasicAuthentication;
            _clientSecretPostAuthentication = clientSecretPostAuthentication;
            _clientAssertionAuthentication = clientAssertionAuthentication;
        }

        public void Authenticate(AuthenticateInstruction instruction)
        {
            
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

            clientId = _clientSecretBasicAuthentication.GetClientId(instruction);
            if (!string.IsNullOrWhiteSpace(clientId))
            {
                return clientId;
            }

            return _clientSecretPostAuthentication.GetClientId(instruction);
        }
    }
}
