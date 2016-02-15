using System;

using Moq;

using NUnit.Framework;

using SimpleIdentityServer.Core.Common;
using SimpleIdentityServer.Core.Factories;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Core.WebSite.Consent.Actions;
using SimpleIdentityServer.Core.Models;
using System.Collections.Generic;
using System.Security.Claims;
using SimpleIdentityServer.Core.Results;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Errors;
using System.Linq;

namespace SimpleIdentityServer.Core.UnitTests.WebSite.Consent
{
    [TestFixture]
    public sealed class DisplayConsentActionFixture
    {
        private Mock<IScopeRepository> _scopeRepositoryFake;

        private Mock<IClientRepository> _clientRepositoryFake;

        private Mock<IConsentHelper> _consentHelperFake;

        private Mock<IGenerateAuthorizationResponse> _generateAuthorizationResponseFake;

        private Mock<IParameterParserHelper> _parameterParserHelperFake;

        private Mock<IActionResultFactory> _actionResultFactoryFake;

        private IDisplayConsentAction _displayConsentAction;

        [Test]
        public void When_Parameter_Is_Null_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var authorizationParameter = new AuthorizationParameter();
            Client client;
            List<Scope> scopes;
            List<string> claims;

            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(() => _displayConsentAction.Execute(
                null, 
                null, 
                out client,
                out scopes,
                out claims));
            Assert.Throws <ArgumentNullException>(() => _displayConsentAction.Execute(
                authorizationParameter,
                null,
                out client,
                out scopes,
                out claims));
        }

        [Test]
        public void When_A_Consent_Has_Been_Given_Then_Redirect_To_Callback()
        {
            // ARRANGE
            InitializeFakeObjects();
            var claimsIdentity = new ClaimsIdentity();
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            Client client;
            List<Scope> scopes;
            List<string> claims;
            var actionResult = new ActionResult
            {
                RedirectInstruction = new RedirectInstruction()
            };
            var authorizationParameter = new AuthorizationParameter
            {
                ResponseMode = ResponseMode.fragment
            };
            var consent = new Core.Models.Consent();
            _consentHelperFake.Setup(c => c.GetConsentConfirmedByResourceOwner(It.IsAny<string>(),
                It.IsAny<AuthorizationParameter>()))
                .Returns(consent);
            _actionResultFactoryFake.Setup(a => a.CreateAnEmptyActionResultWithRedirectionToCallBackUrl())
                .Returns(actionResult);

            // ACT
            actionResult = _displayConsentAction.Execute(authorizationParameter,
                claimsPrincipal,
                out client,
                out scopes,
                out claims);

            // ASSERT
            _actionResultFactoryFake.Verify(a => a.CreateAnEmptyActionResultWithRedirectionToCallBackUrl());
            Assert.IsTrue(actionResult.RedirectInstruction.ResponseMode == ResponseMode.fragment);
        }

        [Test]
        public void When_A_Consent_Has_Been_Given_And_The_AuthorizationFlow_Is_Not_Supported_Then_Exception_Is_Thrown()
        {            
            // ARRANGE
            InitializeFakeObjects();
            const string clientId = "clientId";
            const string state = "state";
            var claimsIdentity = new ClaimsIdentity();
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            Client client;
            List<Scope> scopes;
            List<string> claims;
            var responseTypes = new List<ResponseType>();
            var authorizationParameter = new AuthorizationParameter
            {
                ClientId = clientId,
                State = state,
                ResponseMode = ResponseMode.None // No response mode is defined
            };
            var consent = new Core.Models.Consent();
            _consentHelperFake.Setup(c => c.GetConsentConfirmedByResourceOwner(It.IsAny<string>(),
                It.IsAny<AuthorizationParameter>()))
                .Returns(consent);
            _parameterParserHelperFake.Setup(p => p.ParseResponseType(It.IsAny<string>()))
                .Returns(responseTypes);

            // ACT & ASSERTS
            var exception = Assert.Throws<IdentityServerExceptionWithState>(() => _displayConsentAction.Execute(authorizationParameter,
               claimsPrincipal,
               out client,
               out scopes,
               out claims));
            Assert.IsTrue(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.IsTrue(exception.Message == ErrorDescriptions.TheAuthorizationFlowIsNotSupported);
            Assert.IsTrue(exception.State == state);

        }

        [Test]
        public void When_No_Consent_Has_Been_Given_And_Client_Doesnt_Exist_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string clientId = "clientId";
            const string state = "state";
            var claimsIdentity = new ClaimsIdentity();
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            Client client;
            List<Scope> scopes;
            List<string> claims;
            var authorizationParameter = new AuthorizationParameter
            {
                ClientId = clientId,
                State = state
            };
            _consentHelperFake.Setup(c => c.GetConsentConfirmedByResourceOwner(It.IsAny<string>(),
                It.IsAny<AuthorizationParameter>()))
                .Returns(() => null);
            _clientRepositoryFake.Setup(c => c.GetClientById(It.IsAny<string>())).
                Returns(() => null);

            // ACT & ASSERTS
            var exception = Assert.Throws<IdentityServerExceptionWithState>(() => _displayConsentAction.Execute(authorizationParameter,
               claimsPrincipal,
               out client,
               out scopes,
               out claims));
            Assert.IsTrue(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.IsTrue(exception.Message == string.Format(ErrorDescriptions.ClientIsNotValid, clientId));
            Assert.IsTrue(exception.State == state);
        }

        [Test]
        public void When_No_Consent_Has_Been_Given_Then_Redirect_To_Consent_Screen()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string clientId = "clientId";
            const string state = "state";
            const string scopeName = "profile";
            var claimsIdentity = new ClaimsIdentity();
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            Client client = new Client();
            List<Scope> scopes;
            List<string> claims;
            var authorizationParameter = new AuthorizationParameter
            {
                ClientId = clientId,
                State = state,
                Claims = null,
                Scope = scopeName
            };
            var scope = new Scope
            {
                IsDisplayedInConsent = true,
                Name = scopeName
            };
            _consentHelperFake.Setup(c => c.GetConsentConfirmedByResourceOwner(It.IsAny<string>(),
                It.IsAny<AuthorizationParameter>()))
                .Returns(() => null);
            _clientRepositoryFake.Setup(c => c.GetClientById(It.IsAny<string>())).
                Returns(client);
            _scopeRepositoryFake.Setup(s => s.GetScopeByName(scopeName))
                .Returns(scope);

            // ACT
            _displayConsentAction.Execute(authorizationParameter,
               claimsPrincipal,
               out client,
               out scopes,
               out claims);

            // ASSERTS
            Assert.IsTrue(scopes.Any(s => s.Name == scopeName));
            _actionResultFactoryFake.Verify(a => a.CreateAnEmptyActionResultWithOutput());
        }

        private void InitializeFakeObjects()
        {
            _scopeRepositoryFake = new Mock<IScopeRepository>();
            _clientRepositoryFake = new Mock<IClientRepository>();
            _consentHelperFake = new Mock<IConsentHelper>();
            _generateAuthorizationResponseFake = new Mock<IGenerateAuthorizationResponse>();
            _parameterParserHelperFake = new Mock<IParameterParserHelper>();
            _actionResultFactoryFake = new Mock<IActionResultFactory>();
            _displayConsentAction = new DisplayConsentAction(
                _scopeRepositoryFake.Object,
                _clientRepositoryFake.Object,
                _consentHelperFake.Object,
                _generateAuthorizationResponseFake.Object,
                _parameterParserHelperFake.Object,
                _actionResultFactoryFake.Object);
        }
    }
}
