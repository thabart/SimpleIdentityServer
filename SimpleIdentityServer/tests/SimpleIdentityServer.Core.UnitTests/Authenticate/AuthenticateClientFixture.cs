using Moq;
using SimpleIdentityServer.Core.Authenticate;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Validators;
using SimpleIdentityServer.Logging;
using System;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.Authenticate
{
    public sealed class AuthenticateClientFixture
    {
        private Mock<IClientSecretBasicAuthentication> _clientSecretBasicAuthenticationFake;

        private Mock<IClientSecretPostAuthentication> _clientSecretPostAuthenticationFake;

        private Mock<IClientAssertionAuthentication> _clientAssertionAuthenticationFake;

        private Mock<IClientValidator> _clientValidatorFake;

        private Mock<ISimpleIdentityServerEventSource> _simpleIdentityServerEventSourceFake;

        private IAuthenticateClient _authenticateClient;

        [Fact]
        public void When_Passing_No_Authentication_Instruction_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            string errorMessage;

            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(() => _authenticateClient.Authenticate(null, out errorMessage));
        }

        [Fact]
        public void When_The_ClientId_Cannot_Be_Fetch_Then_Message_Error_Is_Returned_And_Result_Is_Null()
        {
            // ARRANGE
            InitializeFakeObjects();
            var authenticationInstruction = new AuthenticateInstruction();
            string errorMessage;
            _clientAssertionAuthenticationFake.Setup(c => c.GetClientId(It.IsAny<AuthenticateInstruction>()))
                .Returns(string.Empty);

            // ACT
            var client = _authenticateClient.Authenticate(authenticationInstruction, out errorMessage);

            // ASSERTS
            Assert.Null(client);
            Assert.True(errorMessage == ErrorDescriptions.TheClientCannotBeAuthenticated);
        }

        [Fact]
        public void When_The_ClientId_Is_Not_Valid_Then_Message_Error_Is_Returned_And_Result_Is_Null()
        {
            // ARRANGE
            InitializeFakeObjects();
            var authenticationInstruction = new AuthenticateInstruction();
            string errorMessage;
            _clientAssertionAuthenticationFake.Setup(c => c.GetClientId(It.IsAny<AuthenticateInstruction>()))
                .Returns("clientId");
            _clientValidatorFake.Setup(c => c.ValidateClientExist(It.IsAny<string>()))
                .Returns(() => null);

            // ACT
            var client = _authenticateClient.Authenticate(authenticationInstruction, out errorMessage);

            // ASSERTS
            Assert.Null(client);
            Assert.True(errorMessage == ErrorDescriptions.TheClientCannotBeAuthenticated);
        }

        [Fact]
        public void When_Trying_To_Authenticate_The_Client_Via_Secret_Basic_Then_Operation_Is_Called_Client_Is_Returned_And_Events_Are_Logged()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string clientId = "clientId";
            var authenticationInstruction = new AuthenticateInstruction();
            var client = new Client
            {
                TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.client_secret_basic,
                ClientId = clientId
            };

            string errorMessage;
            _clientAssertionAuthenticationFake.Setup(c => c.GetClientId(It.IsAny<AuthenticateInstruction>()))
                .Returns(clientId);
            _clientValidatorFake.Setup(c => c.ValidateClientExist(It.IsAny<string>()))
                .Returns(client);
            _clientSecretBasicAuthenticationFake.Setup(
                c => c.AuthenticateClient(It.IsAny<AuthenticateInstruction>(), It.IsAny<Client>()))
                .Returns(client);

            // ACT
            client = _authenticateClient.Authenticate(authenticationInstruction, out errorMessage);

            // ASSERTS
            Assert.NotNull(client);
            _simpleIdentityServerEventSourceFake.Verify(s => s.StartToAuthenticateTheClient(clientId, "client_secret_basic"));
            _simpleIdentityServerEventSourceFake.Verify(s => s.FinishToAuthenticateTheClient(clientId, "client_secret_basic"));
        }

        [Fact]
        public void When_Trying_To_Authenticate_The_Client_Via_Secret_Basic_But_Operation_Failed_Then_Event_Is_Not_Logged_And_Null_Is_Returned()
        {            
            // ARRANGE
            InitializeFakeObjects();
            const string clientId = "clientId";
            var authenticationInstruction = new AuthenticateInstruction();
            var client = new Client
            {
                TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.client_secret_basic,
                ClientId = clientId
            };

            string errorMessage;
            _clientAssertionAuthenticationFake.Setup(c => c.GetClientId(It.IsAny<AuthenticateInstruction>()))
                .Returns(clientId);
            _clientValidatorFake.Setup(c => c.ValidateClientExist(It.IsAny<string>()))
                .Returns(client);
            _clientSecretBasicAuthenticationFake.Setup(
                c => c.AuthenticateClient(It.IsAny<AuthenticateInstruction>(), It.IsAny<Client>()))
                .Returns(() => null);

            // ACT
            client = _authenticateClient.Authenticate(authenticationInstruction, out errorMessage);

            // ASSERTS
            Assert.Null(client);
            _simpleIdentityServerEventSourceFake.Verify(s => s.StartToAuthenticateTheClient(clientId, "client_secret_basic"));
            _simpleIdentityServerEventSourceFake.Verify(s => s.FinishToAuthenticateTheClient(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        private void InitializeFakeObjects()
        {
            _clientSecretBasicAuthenticationFake = new Mock<IClientSecretBasicAuthentication>();
            _clientSecretPostAuthenticationFake = new Mock<IClientSecretPostAuthentication>();
            _clientAssertionAuthenticationFake = new Mock<IClientAssertionAuthentication>();
            _clientValidatorFake = new Mock<IClientValidator>();
            _simpleIdentityServerEventSourceFake = new Mock<ISimpleIdentityServerEventSource>();
            _authenticateClient = new AuthenticateClient(
                _clientSecretBasicAuthenticationFake.Object,
                _clientSecretPostAuthenticationFake.Object,
                _clientAssertionAuthenticationFake.Object,
                _clientValidatorFake.Object,
                _simpleIdentityServerEventSourceFake.Object);
        }
    }
}
