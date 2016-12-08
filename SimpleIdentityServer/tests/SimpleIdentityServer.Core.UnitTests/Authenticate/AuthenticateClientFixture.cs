using Moq;
using SimpleIdentityServer.Core.Authenticate;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Logging;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.Authenticate
{
    public sealed class AuthenticateClientFixture
    {
        private Mock<IClientSecretBasicAuthentication> _clientSecretBasicAuthenticationFake;
        private Mock<IClientSecretPostAuthentication> _clientSecretPostAuthenticationFake;
        private Mock<IClientAssertionAuthentication> _clientAssertionAuthenticationFake;
        private Mock<IClientRepository> _clientRepositoryStub;
        private Mock<ISimpleIdentityServerEventSource> _simpleIdentityServerEventSourceFake;
        private IAuthenticateClient _authenticateClient;

        [Fact]
        public async Task When_Passing_No_Authentication_Instruction_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            await Assert.ThrowsAsync<AggregateException>(() => _authenticateClient.AuthenticateAsync(null));
        }

        [Fact]
        public async Task When_The_ClientId_Cannot_Be_Fetch_Then_Message_Error_Is_Returned_And_Result_Is_Null()
        {
            // ARRANGE
            InitializeFakeObjects();
            var authenticationInstruction = new AuthenticateInstruction();
            _clientAssertionAuthenticationFake.Setup(c => c.GetClientId(It.IsAny<AuthenticateInstruction>()))
                .Returns(string.Empty);

            // ACT
            var result = await _authenticateClient.AuthenticateAsync(authenticationInstruction);

            // ASSERTS
            Assert.Null(result.Client);
            Assert.True(result.ErrorMessage == ErrorDescriptions.TheClientCannotBeAuthenticated);
        }

        [Fact]
        public async Task When_The_ClientId_Is_Not_Valid_Then_Message_Error_Is_Returned_And_Result_Is_Null()
        {
            // ARRANGE
            InitializeFakeObjects();
            var authenticationInstruction = new AuthenticateInstruction();
            _clientAssertionAuthenticationFake.Setup(c => c.GetClientId(It.IsAny<AuthenticateInstruction>()))
                .Returns("clientId");
            _clientRepositoryStub.Setup(c => c.GetClientByIdAsync(It.IsAny<string>()))
                .Returns(() => Task.FromResult((Client)null));

            // ACT
            var result = await _authenticateClient.AuthenticateAsync(authenticationInstruction);

            // ASSERTS
            Assert.Null(result.Client);
            Assert.True(result.ErrorMessage == ErrorDescriptions.TheClientCannotBeAuthenticated);
        }

        [Fact]
        public async Task When_Trying_To_Authenticate_The_Client_Via_Secret_Basic_Then_Operation_Is_Called_Client_Is_Returned_And_Events_Are_Logged()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string clientId = "clientId";
            var authenticationInstruction = new AuthenticateInstruction();
            var client = new Models.Client
            {
                TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.client_secret_basic,
                ClientId = clientId
            };

            _clientAssertionAuthenticationFake.Setup(c => c.GetClientId(It.IsAny<AuthenticateInstruction>()))
                .Returns(clientId);
            _clientRepositoryStub.Setup(c => c.GetClientByIdAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(client));
            _clientSecretBasicAuthenticationFake.Setup(
                c => c.AuthenticateClient(It.IsAny<AuthenticateInstruction>(), It.IsAny<Models.Client>()))
                .Returns(client);

            // ACT
            var result = await _authenticateClient.AuthenticateAsync(authenticationInstruction);

            // ASSERTS
            Assert.NotNull(result.Client);
            _simpleIdentityServerEventSourceFake.Verify(s => s.StartToAuthenticateTheClient(clientId, "client_secret_basic"));
            _simpleIdentityServerEventSourceFake.Verify(s => s.FinishToAuthenticateTheClient(clientId, "client_secret_basic"));
        }

        [Fact]
        public async Task When_Trying_To_Authenticate_The_Client_Via_Secret_Basic_But_Operation_Failed_Then_Event_Is_Not_Logged_And_Null_Is_Returned()
        {            
            // ARRANGE
            InitializeFakeObjects();
            const string clientId = "clientId";
            var authenticationInstruction = new AuthenticateInstruction();
            var client = new Models.Client
            {
                TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.client_secret_basic,
                ClientId = clientId
            };

            _clientAssertionAuthenticationFake.Setup(c => c.GetClientId(It.IsAny<AuthenticateInstruction>()))
                .Returns(clientId);
            _clientRepositoryStub.Setup(c => c.GetClientByIdAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(client));
            _clientSecretBasicAuthenticationFake.Setup(
                c => c.AuthenticateClient(It.IsAny<AuthenticateInstruction>(), It.IsAny<Models.Client>()))
                .Returns(() => null);

            // ACT
            var result = await _authenticateClient.AuthenticateAsync(authenticationInstruction);
            
            // ASSERTS
            Assert.Null(result.Client);
            _simpleIdentityServerEventSourceFake.Verify(s => s.StartToAuthenticateTheClient(clientId, "client_secret_basic"));
            _simpleIdentityServerEventSourceFake.Verify(s => s.FinishToAuthenticateTheClient(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        private void InitializeFakeObjects()
        {
            _clientSecretBasicAuthenticationFake = new Mock<IClientSecretBasicAuthentication>();
            _clientSecretPostAuthenticationFake = new Mock<IClientSecretPostAuthentication>();
            _clientAssertionAuthenticationFake = new Mock<IClientAssertionAuthentication>();
            _clientRepositoryStub = new Mock<IClientRepository>();
            _simpleIdentityServerEventSourceFake = new Mock<ISimpleIdentityServerEventSource>();
            _authenticateClient = new AuthenticateClient(
                _clientSecretBasicAuthenticationFake.Object,
                _clientSecretPostAuthenticationFake.Object,
                _clientAssertionAuthenticationFake.Object,
                _clientRepositoryStub.Object,
                _simpleIdentityServerEventSourceFake.Object);
        }
    }
}
