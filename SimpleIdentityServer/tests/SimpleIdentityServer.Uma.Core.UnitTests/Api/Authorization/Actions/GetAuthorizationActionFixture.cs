using Moq;
using SimpleIdentityServer.Uma.Core.Api.Authorization.Actions;
using SimpleIdentityServer.Uma.Core.Configuration;
using SimpleIdentityServer.Uma.Core.Errors;
using SimpleIdentityServer.Uma.Core.Exceptions;
using SimpleIdentityServer.Uma.Core.Helpers;
using SimpleIdentityServer.Uma.Core.Parameters;
using SimpleIdentityServer.Uma.Core.Policies;
using SimpleIdentityServer.Uma.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using Xunit;

namespace SimpleIdentityServer.Uma.Core.UnitTests.Api.Authorization.Actions
{
    public class GetAuthorizationActionFixture
    {
        private Mock<ITicketRepository> _ticketRepositoryStub;

        private Mock<IAuthorizationPolicyValidator> _authorizationPolicyValidatorStub;

        private Mock<IUmaServerConfigurationProvider> _umaServerConfigurationProviderStub;

        private Mock<IRptRepository> _rptRepositoryStub;

        private Mock<IRepositoryExceptionHelper> _repositoryExceptionHandlerStub;

        private IGetAuthorizationAction _getAuthorizationAction;

        #region Exceptions

        [Fact]
        public void When_Passing_Null_Parameter_Then_Exceptions_Are_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var getAuthorizationActionParameter = new GetAuthorizationActionParameter();

            // ACT & ASSERTS
            Assert.Throws<ArgumentNullException>(() => _getAuthorizationAction.Execute(null, null));
            Assert.Throws<ArgumentNullException>(() => _getAuthorizationAction.Execute(getAuthorizationActionParameter, null));
        }

        [Fact]
        public void When_TicketId_IsNot_Specified_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var getAuthorizationActionParameter = new GetAuthorizationActionParameter();
            var claims = new List<Claim>
            {
                new Claim("type", "value")
            };

            // ACT & ASSERTS
            var exception = Assert.Throws<BaseUmaException>(() => _getAuthorizationAction.Execute(getAuthorizationActionParameter, claims));
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheParameterNeedsToBeSpecified, "ticket_id"));
        }

        [Fact]
        public void When_Ticket_DoesntExist_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            const string ticketId = "ticket_id";
            InitializeFakeObjects();
            var getAuthorizationActionParameter = new GetAuthorizationActionParameter
            {
                TicketId = ticketId
            };
            var claims = new List<Claim>
            {
                new Claim("type", "value")
            };
            _ticketRepositoryStub.Setup(t => t.GetTicketById(It.IsAny<string>()))
                .Returns(() => null);

            // ACT & ASSERTS
            var exception = Assert.Throws<BaseUmaException>(() => _getAuthorizationAction.Execute(getAuthorizationActionParameter, claims));
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidTicket);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheTicketDoesntExist, ticketId));

        }

        #endregion

        private void InitializeFakeObjects()
        {
            _ticketRepositoryStub = new Mock<ITicketRepository>();
            _authorizationPolicyValidatorStub = new Mock<IAuthorizationPolicyValidator>();
            _umaServerConfigurationProviderStub = new Mock<IUmaServerConfigurationProvider>();
            _rptRepositoryStub = new Mock<IRptRepository>();
            _repositoryExceptionHandlerStub = new Mock<IRepositoryExceptionHelper>();
            _getAuthorizationAction = new GetAuthorizationAction(
                _ticketRepositoryStub.Object,
                _authorizationPolicyValidatorStub.Object,
                _umaServerConfigurationProviderStub.Object,
                _rptRepositoryStub.Object,
                _repositoryExceptionHandlerStub.Object);
        }
    }
}
