using System.Collections.Generic;
using System.Security.Principal;
using Moq;
using Newtonsoft.Json;
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
        [Test]
        public void When_Starting_Authorization_Process_Then_Event_Is_Started_And_Ended()
        {
            // ARRANGE
            var actionResult = new ActionResult
            {
                Type = TypeActionResult.RedirectToAction,
                RedirectInstruction = new RedirectInstruction
                {
                    Action = IdentityServerEndPoints.ConsentIndex
                }
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
            authorizationActions.GetAuthorization(authorizationParameter, null, string.Empty);

            // ASSERTS
            simpleIdentityServerEventSourceMock.Verify(s => s.StartAuthorization(clientId, responseType, scope, string.Empty));
            simpleIdentityServerEventSourceMock.Verify(s => s.EndAuthorization(actionType, controllerAction, serializedParameter));
        }
    }
}
