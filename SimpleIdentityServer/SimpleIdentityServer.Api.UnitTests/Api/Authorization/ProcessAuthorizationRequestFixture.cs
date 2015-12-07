using System;
using System.Linq;
using NUnit.Framework;
using SimpleIdentityServer.Api.UnitTests.Fake;
using SimpleIdentityServer.Core.Api.Authorization.Common;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Factories;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Validators;
using SimpleIdentityServer.DataAccess.Fake;
using SimpleIdentityServer.DataAccess.Fake.Models;

namespace SimpleIdentityServer.Api.UnitTests.Api.Authorization
{
    [TestFixture]
    public sealed class ProcessAuthorizationRequestFixture : BaseFixture
    {
        private ProcessAuthorizationRequest _processAuthorizationRequest;

        [Test]
        public void When_Passing_NullAuthorization_To_Function_Then_ArgumentNullException_IsRaised()
        {
            // ARRANGE
            InitializeMockingObjects();

            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(() => _processAuthorizationRequest.Process(null, null, string.Empty));
        }

        [Test]
        public void When_Passing_NoEncryptedRequest_To_Function_Then_ArgumentNullException_IsRaised()
        {
            // ARRANGE
            InitializeMockingObjects();
            var authorizationParameter = new AuthorizationParameter();

            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(() => _processAuthorizationRequest.Process(authorizationParameter, null, string.Empty));
        }

        [Test]
        public void When_Passing_NoneExisting_ClientId_To_AuthorizationParameter_Then_Exception_Is_Raised()
        {
            // ARRANGE
            InitializeMockingObjects();
            const string state = "state";
            const string code = "code";
            const string clientId = "fake_client";
            var authorizationParameter = new AuthorizationParameter
            {
                ClientId = "fake_client",
                Prompt = "login",
                State = state
            };

            // ACT & ASSERTS
            var exception =
                Assert.Throws<IdentityServerExceptionWithState>(
                    () => _processAuthorizationRequest.Process(authorizationParameter, null, code));
            Assert.That(exception.Code, Is.EqualTo(ErrorCodes.InvalidClient));
            Assert.That(exception.Message, Is.EqualTo(string.Format(ErrorDescriptions.ClientIsNotValid, clientId)));
            Assert.That(exception.State, Is.EqualTo(state));
        }

        [Test]
        public void When_Passing_NotValidRedirectUrl_To_AuthorizationParameter_Then_Exception_Is_Raised()
        {
            // ARRANGE
            InitializeMockingObjects();
            const string state = "state";
            const string code = "code";
            const string clientId = "MyBlog";
            const string redirectUrl = "not valid redirect url";
            var authorizationParameter = new AuthorizationParameter
            {
                ClientId = clientId,
                Prompt = "login",
                State = state,
                RedirectUrl = redirectUrl
            };

            // ACT & ASSERTS
            var exception =
                Assert.Throws<IdentityServerExceptionWithState>(
                    () => _processAuthorizationRequest.Process(authorizationParameter, null, code));
            Assert.That(exception.Code, Is.EqualTo(ErrorCodes.InvalidRequestCode));
            Assert.That(exception.Message, Is.EqualTo(string.Format(ErrorDescriptions.RedirectUrlIsNotValid, redirectUrl)));
            Assert.That(exception.State, Is.EqualTo(state));
        }

        [Test]
        public void When_Passing_AuthorizationParameterWithoutOpenIdScope_Then_Exception_Is_Raised()
        {
            // ARRANGE
            InitializeMockingObjects();
            const string state = "state";
            const string code = "code";
            const string clientId = "MyBlog";
            const string redirectUrl = "http://localhost";
            var authorizationParameter = new AuthorizationParameter
            {
                ClientId = clientId,
                Prompt = "login",
                State = state,
                RedirectUrl = redirectUrl,
                Scope = "email"
            };

            // ACT & ASSERTS
            var exception =
                Assert.Throws<IdentityServerExceptionWithState>(
                    () => _processAuthorizationRequest.Process(authorizationParameter, null, code));
            Assert.That(exception.Code, Is.EqualTo(ErrorCodes.InvalidScope));
            Assert.That(exception.Message, Is.EqualTo(string.Format(ErrorDescriptions.TheScopesNeedToBeSpecified, Core.Constants.StandardScopes.OpenId.Name)));
            Assert.That(exception.State, Is.EqualTo(state));
        }

        [Test]
        public void When_Passing_AuthorizationRequestWithMissingResponseType_Then_Exception_Is_Raised()
        {
            // ARRANGE
            InitializeMockingObjects();
            const string state = "state";
            const string code = "code";
            const string clientId = "MyBlog";
            const string redirectUrl = "http://localhost";
            var authorizationParameter = new AuthorizationParameter
            {
                ClientId = clientId,
                Prompt = "login",
                State = state,
                RedirectUrl = redirectUrl,
                Scope = "openid"
            };

            // ACT & ASSERTS
            var exception =
                Assert.Throws<IdentityServerExceptionWithState>(
                    () => _processAuthorizationRequest.Process(authorizationParameter, null, code));
            Assert.That(exception.Code, Is.EqualTo(ErrorCodes.InvalidRequestCode));
            Assert.That(exception.Message, Is.EqualTo(string.Format(ErrorDescriptions.MissingParameter
                , Core.Constants.StandardAuthorizationRequestParameterNames.ResponseTypeName)));
            Assert.That(exception.State, Is.EqualTo(state));
        }

        [Test]
        public void When_Passing_AuthorizationRequestWithNotSupportedResponseType_Then_Exception_Is_Raised()
        {
            // ARRANGE
            InitializeMockingObjects();
            const string state = "state";
            const string code = "code";
            const string clientId = "MyBlog";
            const string redirectUrl = "http://localhost";
            var authorizationParameter = new AuthorizationParameter
            {
                ClientId = clientId,
                Prompt = "login",
                State = state,
                RedirectUrl = redirectUrl,
                Scope = "openid",
                ResponseType = "code"
            };

            // ACT
            var client = FakeDataSource.Instance().Clients.FirstOrDefault(c => c.ClientId == clientId);
            Assert.IsNotNull(client);
            client.ResponseTypes.Remove(ResponseType.code);

            // ACT & ASSERTS
            var exception =
                Assert.Throws<IdentityServerExceptionWithState>(
                    () => _processAuthorizationRequest.Process(authorizationParameter, null, code));
            Assert.That(exception.Code, Is.EqualTo(ErrorCodes.InvalidRequestCode));
            Assert.That(exception.Message, Is.EqualTo(string.Format(ErrorDescriptions.TheClientDoesntSupportTheResponseType
                , clientId, "code")));
            Assert.That(exception.State, Is.EqualTo(state));
        }

        private void InitializeMockingObjects()
        {
            var scopeRepository = FakeFactories.GetScopeRepository();
            var clientRepository = FakeFactories.GetClientRepository();
            var consentRepository = FakeFactories.GetConsentRepository();
            var parameterParserHelper = new ParameterParserHelper(scopeRepository);
            var clientValidator = new ClientValidator(clientRepository);
            var scopeValidator = new ScopeValidator(parameterParserHelper);
            var actionResultFactory = new ActionResultFactory();
            var consentHelper = new ConsentHelper(consentRepository, parameterParserHelper);
            _processAuthorizationRequest = new ProcessAuthorizationRequest(
                parameterParserHelper,
                clientValidator,
                scopeValidator,
                actionResultFactory,
                consentHelper);
        }

        [OneTimeTearDown]
        public void CleanFakeData()
        {
            FakeDataSource.Instance().Init();
        }
    }
}
