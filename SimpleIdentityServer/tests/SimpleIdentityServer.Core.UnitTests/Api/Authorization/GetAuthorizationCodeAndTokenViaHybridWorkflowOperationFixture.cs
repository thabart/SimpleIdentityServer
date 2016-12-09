using System;
using System.Security.Claims;
using Moq;
using SimpleIdentityServer.Core.Api.Authorization.Actions;
using SimpleIdentityServer.Core.Api.Authorization.Common;
using SimpleIdentityServer.Core.Common;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Results;
using SimpleIdentityServer.Core.Validators;
using SimpleIdentityServer.Logging;
using Xunit;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Core.UnitTests.Api.Authorization
{
    public sealed class GetAuthorizationCodeAndTokenViaHybridWorkflowOperationFixture
    {
        private Mock<ISimpleIdentityServerEventSource> _simpleIdentityServerEventSourceFake;
        private Mock<IProcessAuthorizationRequest> _processAuthorizationRequestFake;
        private Mock<IClientValidator> _clientValidatorFake;
        private Mock<IGenerateAuthorizationResponse> _generateAuthorizationResponseFake;

        private IGetAuthorizationCodeAndTokenViaHybridWorkflowOperation
            _getAuthorizationCodeAndTokenViaHybridWorkflowOperation;

        [Fact]
        public async Task When_Passing_Null_Parameters_Then_Exceptions_Are_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            await Assert.ThrowsAsync<ArgumentNullException>(() => _getAuthorizationCodeAndTokenViaHybridWorkflowOperation.Execute(null, null, null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => _getAuthorizationCodeAndTokenViaHybridWorkflowOperation.Execute(new AuthorizationParameter(), null, null));
        }

        [Fact]
        public async Task When_Nonce_Parameter_Is_Not_Set_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var authorizationParameter = new AuthorizationParameter
            {
                State = "state"
            };

            // ACT & ASSERT
            var ex = await Assert.ThrowsAsync<IdentityServerExceptionWithState>(() => _getAuthorizationCodeAndTokenViaHybridWorkflowOperation.Execute(authorizationParameter, null, new Client()));
            Assert.True(ex.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(ex.Message == string.Format(ErrorDescriptions.MissingParameter, Constants.StandardAuthorizationRequestParameterNames.NonceName));
            Assert.True(ex.State == authorizationParameter.State);
        }

        [Fact]
        public async Task When_Grant_Type_Is_Not_Supported_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var authorizationParameter = new AuthorizationParameter
            {
                State = "state",
                Nonce = "nonce"
            };

            _clientValidatorFake.Setup(c => c.CheckGrantTypes(It.IsAny<Models.Client>(), It.IsAny<GrantType[]>())).Returns(false);

            // ACT & ASSERT
            var ex = await Assert.ThrowsAsync<IdentityServerExceptionWithState>(
                () => _getAuthorizationCodeAndTokenViaHybridWorkflowOperation.Execute(authorizationParameter, null, new Client()));
            Assert.True(ex.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(ex.Message == string.Format(ErrorDescriptions.TheClientDoesntSupportTheGrantType,
                        authorizationParameter.ClientId,
                        "implicit"));
            Assert.True(ex.State == authorizationParameter.State);
        }

        [Fact]
        public async Task When_Redirected_To_Callback_And_Resource_Owner_Is_Not_Authenticated_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var authorizationParameter = new AuthorizationParameter
            {
                State = "state",
                Nonce = "nonce"
            };

            var actionResult = new ActionResult
            {
                Type = TypeActionResult.RedirectToCallBackUrl
            };

            _processAuthorizationRequestFake.Setup(p => p.ProcessAsync(It.IsAny<AuthorizationParameter>(), It.IsAny<ClaimsPrincipal>(), It.IsAny<Client>()))
                .Returns(Task.FromResult(actionResult));
            _clientValidatorFake.Setup(c => c.CheckGrantTypes(It.IsAny<Models.Client>(), It.IsAny<GrantType[]>()))
                .Returns(true);

            // ACT & ASSERT
            var ex = await Assert.ThrowsAsync<IdentityServerExceptionWithState>(
                () => _getAuthorizationCodeAndTokenViaHybridWorkflowOperation.Execute(authorizationParameter, null, new Client()));
            Assert.True(ex.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(ex.Message ==
                          ErrorDescriptions.TheResponseCannotBeGeneratedBecauseResourceOwnerNeedsToBeAuthenticated);
            Assert.True(ex.State == authorizationParameter.State);
        }

        [Fact]
        public async Task When_Resource_Owner_Is_Authenticated_And_Pass_Correct_Authorization_Request_Then_Events_Are_Logged()
        {
            // ARRANGE
            InitializeFakeObjects();
            var authorizationParameter = new AuthorizationParameter
            {
                State = "state",
                ClientId = "client_id",
                Scope = "scope",
                Nonce = "nonce"
            };

            var actionResult = new ActionResult
            {
                Type = TypeActionResult.RedirectToCallBackUrl,
                RedirectInstruction = new RedirectInstruction
                {
                    Action = IdentityServerEndPoints.AuthenticateIndex
                }
            };

            var claimsPrincipal = new ClaimsPrincipal();

            _processAuthorizationRequestFake.Setup(p => p.ProcessAsync(It.IsAny<AuthorizationParameter>(), It.IsAny<ClaimsPrincipal>(), It.IsAny<Client>()))
                .Returns(Task.FromResult(actionResult));
            _clientValidatorFake.Setup(c => c.CheckGrantTypes(It.IsAny<Models.Client>(), It.IsAny<GrantType[]>()))
                .Returns(true);

            // ACT
            await _getAuthorizationCodeAndTokenViaHybridWorkflowOperation.Execute(authorizationParameter, claimsPrincipal, new Client());
            _simpleIdentityServerEventSourceFake.Verify(s => s.StartHybridFlow(authorizationParameter.ClientId,
                authorizationParameter.Scope,
                string.Empty));
            _generateAuthorizationResponseFake.Verify(g => g.ExecuteAsync(actionResult, authorizationParameter, claimsPrincipal, It.IsAny<Client>()));
            _simpleIdentityServerEventSourceFake.Verify(s => s.EndHybridFlow(authorizationParameter.ClientId,
                Enum.GetName(typeof(TypeActionResult), actionResult.Type),
                Enum.GetName(typeof(IdentityServerEndPoints), actionResult.RedirectInstruction.Action)));
        }

        private void InitializeFakeObjects()
        {
            _simpleIdentityServerEventSourceFake = new Mock<ISimpleIdentityServerEventSource>();
            _processAuthorizationRequestFake = new Mock<IProcessAuthorizationRequest>();
            _clientValidatorFake = new Mock<IClientValidator>();
            _generateAuthorizationResponseFake = new Mock<IGenerateAuthorizationResponse>();
            _getAuthorizationCodeAndTokenViaHybridWorkflowOperation = new GetAuthorizationCodeAndTokenViaHybridWorkflowOperation(
                _simpleIdentityServerEventSourceFake.Object,
                _processAuthorizationRequestFake.Object,
                _clientValidatorFake.Object,
                _generateAuthorizationResponseFake.Object);
        }
    }
}
