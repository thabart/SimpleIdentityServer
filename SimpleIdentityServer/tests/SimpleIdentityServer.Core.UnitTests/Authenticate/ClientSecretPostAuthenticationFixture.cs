using System;
using SimpleIdentityServer.Core.Authenticate;
using SimpleIdentityServer.Core.Models;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.Authenticate
{
    public sealed class ClientSecretPostAuthenticationFixture
    {
        private IClientSecretPostAuthentication _clientSecretPostAuthentication;

        [Fact]
        public void When_Trying_To_Authenticate_The_Client_And_OneParameter_Is_Null_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var authenticateInstruction = new AuthenticateInstruction();

            // ACT & ASSERTS
            Assert.Throws<ArgumentNullException>(() => _clientSecretPostAuthentication.AuthenticateClient(null, null));
            Assert.Throws<ArgumentNullException>(() => _clientSecretPostAuthentication.AuthenticateClient(authenticateInstruction, null));
        }

        [Fact]
        public void When_Trying_To_Authenticate_The_Client_And_Credentials_Are_Not_Correct_Then_Null_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            var authenticateInstruction = new AuthenticateInstruction
            {
                ClientSecretFromHttpRequestBody = "notCorrectClientSecret"
            };
            var client = new Client
            {
                ClientSecret = "clientSecret"
            };

            // ACT
            client = _clientSecretPostAuthentication.AuthenticateClient(authenticateInstruction, client);

            // ASSERT
            Assert.Null(client);
        }

        [Fact]
        public void When_Trying_To_Authenticate_The_Client_And_Credentials_Are_Correct_Then_Client_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string clientSecret = "clientSecret";
            var authenticateInstruction = new AuthenticateInstruction
            {
                ClientSecretFromHttpRequestBody = clientSecret
            };
            var client = new Client
            {
                ClientSecret = clientSecret
            };

            // ACT
            client = _clientSecretPostAuthentication.AuthenticateClient(authenticateInstruction, client);

            // ASSERT
            Assert.NotNull(client);
            Assert.True(client.ClientSecret == authenticateInstruction.ClientSecretFromHttpRequestBody);
        }

        [Fact]
        public void When_Requesting_ClientId_And_Instruction_Is_Null_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(() => _clientSecretPostAuthentication.GetClientId(null));
        }

        [Fact]
        public void When_Requesting_ClientId_Then_ClientId_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string clientId = "clientId";
            var instruction = new AuthenticateInstruction
            {
                ClientIdFromHttpRequestBody = clientId
            };

            // ACT
            var result = _clientSecretPostAuthentication.GetClientId(instruction);

            // ASSERT
            Assert.True(clientId == result);

        }

        private void InitializeFakeObjects()
        {
            _clientSecretPostAuthentication = new ClientSecretPostAuthentication();   
        }
    }
}
