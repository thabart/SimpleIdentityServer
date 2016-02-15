using System.Collections.Generic;
using NUnit.Framework;
using SimpleIdentityServer.Core.Api.Authorization;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.Models;

namespace SimpleIdentityServer.Core.UnitTests.Helpers
{
    [TestFixture]
    public sealed class AuthorizationFlowHelperFixture
    {
        private IAuthorizationFlowHelper _authorizationFlowHelper;

        [Test]
        public void When_Passing_No_Response_Type_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            const string state = "state";
            InitializeFakeObjects();

            // ACT & ASSERTS
            var exception = Assert.Throws<IdentityServerExceptionWithState>(() => _authorizationFlowHelper.GetAuthorizationFlow(null, state));
            Assert.IsTrue(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.IsTrue(exception.Message == ErrorDescriptions.TheAuthorizationFlowIsNotSupported);
            Assert.IsTrue(exception.State == state);
        }

        [Test]
        public void When_Passing_Empty_List_Of_Response_Types_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            const string state = "state";
            InitializeFakeObjects();

            // ACT & ASSERTS
            var exception = Assert.Throws<IdentityServerExceptionWithState>(() => _authorizationFlowHelper.GetAuthorizationFlow(
                new List<ResponseType>(),
                state));
            Assert.IsTrue(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.IsTrue(exception.Message == ErrorDescriptions.TheAuthorizationFlowIsNotSupported);
            Assert.IsTrue(exception.State == state);
        }

        [Test]
        public void When_Passing_Code_Then_Authorization_Code_Flow_Should_Be_Returned()
        {
            // ARRANGE
            const string state = "state";
            InitializeFakeObjects();

            // ACT
            var result = _authorizationFlowHelper.GetAuthorizationFlow(
                new List<ResponseType> { ResponseType.code },
                state);

            // ASSERT
            Assert.IsTrue(result == AuthorizationFlow.AuthorizationCodeFlow);
        }

        [Test]
        public void When_Passing_Id_Token_Then_Implicit_Flow_Should_Be_Returned()
        {
            // ARRANGE
            const string state = "state";
            InitializeFakeObjects();

            // ACT
            var result = _authorizationFlowHelper.GetAuthorizationFlow(
                new List<ResponseType> { ResponseType.id_token },
                state);

            // ASSERT
            Assert.IsTrue(result == AuthorizationFlow.ImplicitFlow);
        }

        [Test]
        public void When_Passing_Code_And_Id_Token_Then_Hybrid_Flow_Should_Be_Returned()
        {
            // ARRANGE
            const string state = "state";
            InitializeFakeObjects();

            // ACT
            var result = _authorizationFlowHelper.GetAuthorizationFlow(
                new List<ResponseType> { ResponseType.id_token, ResponseType.code },
                state);

            // ASSERT
            Assert.IsTrue(result == AuthorizationFlow.HybridFlow);
        }

        private void InitializeFakeObjects()
        {
            _authorizationFlowHelper = new AuthorizationFlowHelper();
        }
    }
}
