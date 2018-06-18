using SimpleIdentityServer.Core.Api.Authorization;
using SimpleIdentityServer.Core.Common.Models;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Helpers;
using System.Collections.Generic;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.Helpers
{
    public sealed class AuthorizationFlowHelperFixture
    {
        private IAuthorizationFlowHelper _authorizationFlowHelper;

        [Fact]
        public void When_Passing_No_Response_Type_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            const string state = "state";
            InitializeFakeObjects();

            // ACT & ASSERTS
            var exception = Assert.Throws<IdentityServerExceptionWithState>(() => _authorizationFlowHelper.GetAuthorizationFlow(null, state));
            Assert.True(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(exception.Message == ErrorDescriptions.TheAuthorizationFlowIsNotSupported);
            Assert.True(exception.State == state);
        }

        [Fact]
        public void When_Passing_Empty_List_Of_Response_Types_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            const string state = "state";
            InitializeFakeObjects();

            // ACT & ASSERTS
            var exception = Assert.Throws<IdentityServerExceptionWithState>(() => _authorizationFlowHelper.GetAuthorizationFlow(
                new List<ResponseType>(),
                state));
            Assert.True(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(exception.Message == ErrorDescriptions.TheAuthorizationFlowIsNotSupported);
            Assert.True(exception.State == state);
        }

        [Fact]
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
            Assert.True(result == AuthorizationFlow.AuthorizationCodeFlow);
        }

        [Fact]
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
            Assert.True(result == AuthorizationFlow.ImplicitFlow);
        }

        [Fact]
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
            Assert.True(result == AuthorizationFlow.HybridFlow);
        }

        private void InitializeFakeObjects()
        {
            _authorizationFlowHelper = new AuthorizationFlowHelper();
        }
    }
}
