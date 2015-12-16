using System;

using NUnit.Framework;

using SimpleIdentityServer.Core.Authenticate;
using SimpleIdentityServer.Core.Models;

namespace SimpleIdentityServer.Api.UnitTests.Authenticate
{
    [TestFixture]
    public sealed class ClientSecretBasicAuthenticationFixture
    {
        private IClientSecretBasicAuthentication _clientSecretBasicAuthentication;

        [Test]
        public void When_Trying_To_Authenticate_The_Client_And_OneParameter_Is_Null_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var authenticateInstruction = new AuthenticateInstruction();

            // ACT & ASSERTS
            Assert.Throws<ArgumentNullException>(() => _clientSecretBasicAuthentication.AuthenticateClient(null, null));
            Assert.Throws<ArgumentNullException>(() => _clientSecretBasicAuthentication.AuthenticateClient(authenticateInstruction, null));
        }

        [Test]
        public void When_Trying_To_Authenticate_The_Client_And_Credentials_Are_Not_Correct_Then_Null_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            var authenticateInstruction = new AuthenticateInstruction
            {
                ClientSecretFromAuthorizationHeader= "notCorrectClientSecret"
            };
            var client = new Client
            {
                ClientSecret = "clientSecret"
            };

            // ACT
            client = _clientSecretBasicAuthentication.AuthenticateClient(authenticateInstruction, client);

            // ASSERT
            Assert.IsNull(client);
        }

        [Test]
        public void When_Trying_To_Authenticate_The_Client_And_Credentials_Are_Correct_Then_Client_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string clientSecret = "clientSecret";
            var authenticateInstruction = new AuthenticateInstruction
            {
                ClientSecretFromAuthorizationHeader = clientSecret
            };
            var client = new Client
            {
                ClientSecret = clientSecret
            };

            // ACT
            client = _clientSecretBasicAuthentication.AuthenticateClient(authenticateInstruction, client);

            // ASSERT
            Assert.IsNotNull(client);
            Assert.IsTrue(client.ClientSecret == authenticateInstruction.ClientSecretFromAuthorizationHeader);
        }

        [Test]
        public void When_Requesting_ClientId_And_Instruction_Is_Null_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(() => _clientSecretBasicAuthentication.GetClientId(null));
        }

        [Test]
        public void When_Requesting_ClientId_Then_ClientId_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string clientId = "clientId";
            var instruction = new AuthenticateInstruction
            {
                ClientIdFromAuthorizationHeader = clientId
            };

            // ACT
            var result = _clientSecretBasicAuthentication.GetClientId(instruction);
            
            // ASSERT
            Assert.IsTrue(clientId == result);

        }

        private void InitializeFakeObjects()
        {
            _clientSecretBasicAuthentication = new ClientSecretBasicAuthentication();
        }
    }
}