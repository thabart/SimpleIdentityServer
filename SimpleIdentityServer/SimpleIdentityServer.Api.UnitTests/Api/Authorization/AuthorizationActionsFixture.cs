using System.Collections.Generic;
using System.Security.Principal;
using Moq;
using NUnit.Framework;
using SimpleIdentityServer.Core.Api.Authorization;
using SimpleIdentityServer.Core.Api.Authorization.Actions;
using SimpleIdentityServer.Core.Common.Extensions;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Results;
using SimpleIdentityServer.Core.Validators;
using SimpleIdentityServer.Logging;

namespace SimpleIdentityServer.Api.UnitTests.Api.Authorization
{
    [TestFixture]
    public sealed class AuthorizationActionsFixture : BaseFixture
    {
        private Mock<IGetAuthorizationCodeOperation> _getAuthorizationCodeOperationFake;

        private Mock<IGetTokenViaImplicitWorkflowOperation> _getTokenViaImplicitWorkflowOperationFake;

        private Mock<IAuthorizationCodeGrantTypeParameterAuthEdpValidator> _authorizationCodeGrantTypeParameterAuthEdpValidatorFake;

        private Mock<IParameterParserHelper> _parameterParserHelperFake;

        private Mock<ISimpleIdentityServerEventSource> _simpleIdentityServerEventSourceFake;

        private IAuthorizationActions _authorizationActions;

        [Test]
        public void When_Starting_Authorization_Process_Then_Event_Is_Started_And_Ended()
        {
            // ARRANGE
            InitializeFakeObjects();
            var actionResult = new ActionResult
            {
                Type = TypeActionResult.RedirectToAction,
                RedirectInstruction = new RedirectInstruction
                {
                    Action = IdentityServerEndPoints.ConsentIndex
                }
            };

            _parameterParserHelperFake.Setup(p => p.ParseResponseType(It.IsAny<string>()))
                .Returns(new List<ResponseType>
                {
                    ResponseType.id_token
                });
            _getTokenViaImplicitWorkflowOperationFake.Setup(g => g.Execute(It.IsAny<AuthorizationParameter>(),
                It.IsAny<IPrincipal>(), It.IsAny<string>())).Returns(actionResult);

            const string clientId = "clientId";
            const string responseType = "id_token";
            const string scope = "openid";
            const string actionType = "RedirectToAction";
            const string controllerAction = "ConsentIndex";

            var authorizationParameter = new AuthorizationParameter
            {
                ClientId = clientId,
                ResponseType = responseType,
                Scope = scope,
                Claims = null
            };
            var serializedParameter = actionResult.RedirectInstruction.Parameters.SerializeWithJavascript();

            // ACT
            _authorizationActions.GetAuthorization(authorizationParameter, null, string.Empty);

            // ASSERTS
            _simpleIdentityServerEventSourceFake.Verify(s => s.StartAuthorization(clientId, responseType, scope, string.Empty));
            _simpleIdentityServerEventSourceFake.Verify(s => s.EndAuthorization(actionType, controllerAction, serializedParameter));
        }

        [Test]
        public void When_Requesting_Authorization_And_Action_Result_Is_Redirected_To_Callback_Then_ResponseMode_Is_Specified()
        {
            // ARRANGE
            var actionResult = new ActionResult
            {
                Type = TypeActionResult.RedirectToCallBackUrl,
                RedirectInstruction = new RedirectInstruction()
            };
            var getAuthorizationCodeOperationStub = new Mock<IGetAuthorizationCodeOperation>();
            var getTokenViaImplicitWorkflowOperationStub = new Mock<IGetTokenViaImplicitWorkflowOperation>();
            var authorizationCodeGrantTypeParameterValidatorStub =
                new Mock<IAuthorizationCodeGrantTypeParameterAuthEdpValidator>();
            var parameterParserHelperStub = new Mock<IParameterParserHelper>();
            var simpleIdentityServerEventSourceMock = new Mock<ISimpleIdentityServerEventSource>();
            var authorizationActions = new AuthorizationActions(
                getAuthorizationCodeOperationStub.Object,
                getTokenViaImplicitWorkflowOperationStub.Object,
                authorizationCodeGrantTypeParameterValidatorStub.Object,
                parameterParserHelperStub.Object,
                simpleIdentityServerEventSourceMock.Object);
            parameterParserHelperStub.Setup(p => p.ParseResponseType(It.IsAny<string>()))
                .Returns(new List<ResponseType>
                {
                    ResponseType.id_token
                });
            getTokenViaImplicitWorkflowOperationStub.Setup(g => g.Execute(It.IsAny<AuthorizationParameter>(),
                It.IsAny<IPrincipal>(), It.IsAny<string>())).Returns(actionResult);

            const string clientId = "clientId";
            const string responseType = "id_token";
            const string scope = "openid";
            const string actionType = "RedirectToCallBackUrl";

            var authorizationParameter = new AuthorizationParameter
            {
                ClientId = clientId,
                ResponseType = responseType,
                Scope = scope,
                Claims = null
            };
            var serializedParameter = actionResult.RedirectInstruction.Parameters.SerializeWithJavascript();

            // ACT
            var result = authorizationActions.GetAuthorization(authorizationParameter, null, string.Empty);

            // ASSERTS
            simpleIdentityServerEventSourceMock.Verify(s => s.StartAuthorization(clientId, responseType, scope, string.Empty));
            simpleIdentityServerEventSourceMock.Verify(s => s.EndAuthorization(actionType, string.Empty, serializedParameter));
            Assert.IsTrue(result.RedirectInstruction.ResponseMode == ResponseMode.fragment);
        }

        private void InitializeFakeObjects()
        {
            _getAuthorizationCodeOperationFake = new Mock<IGetAuthorizationCodeOperation>();
            _getTokenViaImplicitWorkflowOperationFake = new Mock<IGetTokenViaImplicitWorkflowOperation>();
            _authorizationCodeGrantTypeParameterAuthEdpValidatorFake =
                new Mock<IAuthorizationCodeGrantTypeParameterAuthEdpValidator>();
            _parameterParserHelperFake = new Mock<IParameterParserHelper>();
            _simpleIdentityServerEventSourceFake = new Mock<ISimpleIdentityServerEventSource>();
            _authorizationActions = new AuthorizationActions(
                _getAuthorizationCodeOperationFake.Object,
                _getTokenViaImplicitWorkflowOperationFake.Object,
                _authorizationCodeGrantTypeParameterAuthEdpValidatorFake.Object,
                _parameterParserHelperFake.Object,
                _simpleIdentityServerEventSourceFake.Object);
        }
    }
}
