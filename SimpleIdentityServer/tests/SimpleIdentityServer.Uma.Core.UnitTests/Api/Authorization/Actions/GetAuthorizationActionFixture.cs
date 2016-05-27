#region copyright
// Copyright 2015 Habart Thierry
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using Moq;
using SimpleIdentityServer.Uma.Core.Api.Authorization.Actions;
using SimpleIdentityServer.Uma.Core.Errors;
using SimpleIdentityServer.Uma.Core.Exceptions;
using SimpleIdentityServer.Uma.Core.Helpers;
using SimpleIdentityServer.Uma.Core.Models;
using SimpleIdentityServer.Uma.Core.Parameters;
using SimpleIdentityServer.Uma.Core.Policies;
using SimpleIdentityServer.Uma.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Uma.Core.UnitTests.Api.Authorization.Actions
{
    public class GetAuthorizationActionFixture
    {
        private Mock<ITicketRepository> _ticketRepositoryStub;

        private Mock<IAuthorizationPolicyValidator> _authorizationPolicyValidatorStub;

        private Mock<UmaServerOptions> _umaServerOptionsStub;

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
            Assert.ThrowsAsync<ArgumentNullException>(() => _getAuthorizationAction.Execute(null, null));
            Assert.ThrowsAsync<ArgumentNullException>(() => _getAuthorizationAction.Execute(getAuthorizationActionParameter, null));
        }

        [Fact]
        public async Task When_TicketId_IsNot_Specified_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var getAuthorizationActionParameter = new GetAuthorizationActionParameter();
            var claims = new List<System.Security.Claims.Claim>
            {
                new System.Security.Claims.Claim("type", "value")
            };

            // ACT & ASSERTS
            var exception = await Assert.ThrowsAsync<BaseUmaException>(() => _getAuthorizationAction.Execute(getAuthorizationActionParameter, claims));
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheParameterNeedsToBeSpecified, "ticket_id"));
        }

        [Fact]
        public async Task When_Ticket_DoesntExist_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            const string ticketId = "ticket_id";
            InitializeFakeObjects();
            var getAuthorizationActionParameter = new GetAuthorizationActionParameter
            {
                TicketId = ticketId
            };
            var claims = new List<System.Security.Claims.Claim>
            {
                new System.Security.Claims.Claim("type", "value")
            };
            _ticketRepositoryStub.Setup(t => t.GetTicketById(It.IsAny<string>()))
                .Returns(() => null);

            // ACT & ASSERTS
            var exception = await Assert.ThrowsAsync<BaseUmaException>(() => _getAuthorizationAction.Execute(getAuthorizationActionParameter, claims));
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidTicket);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheTicketDoesntExist, ticketId));

        }

        [Fact]
        public async Task When_TicketClientId_Is_Different_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            const string ticketId = "ticket_id";
            const string clientId = "client_id";
            InitializeFakeObjects();
            var getAuthorizationActionParameter = new GetAuthorizationActionParameter
            {
                TicketId = ticketId
            };
            var claims = new List<System.Security.Claims.Claim>
            {
                new System.Security.Claims.Claim("type", "value"),
                new System.Security.Claims.Claim("client_id", "invalid_client_id")
            };
            var ticket = new Ticket
            {
                ClientId = clientId
            };
            _ticketRepositoryStub.Setup(t => t.GetTicketById(It.IsAny<string>()))
                .Returns(ticket);

            // ACT & ASSERTS
            var exception = await Assert.ThrowsAsync<BaseUmaException>(() => _getAuthorizationAction.Execute(getAuthorizationActionParameter, claims));
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidTicket);
            Assert.True(exception.Message == ErrorDescriptions.TheTicketIssuerIsDifferentFromTheClient);
        }

        [Fact]
        public async Task When_TicketIsExpired_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            const string ticketId = "ticket_id";
            const string clientId = "client_id";
            InitializeFakeObjects();
            var getAuthorizationActionParameter = new GetAuthorizationActionParameter
            {
                TicketId = ticketId
            };
            var claims = new List<System.Security.Claims.Claim>
            {
                new System.Security.Claims.Claim("type", "value"),
                new System.Security.Claims.Claim("client_id", clientId)
            };
            var ticket = new Ticket
            {
                ClientId = clientId,
                ExpirationDateTime = DateTime.UtcNow.AddSeconds(-40)
            };
            _ticketRepositoryStub.Setup(t => t.GetTicketById(It.IsAny<string>()))
                .Returns(ticket);

            // ACT & ASSERTS
            var exception = await Assert.ThrowsAsync<BaseUmaException>(() => _getAuthorizationAction.Execute(getAuthorizationActionParameter, claims));
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.ExpiredTicket);
            Assert.True(exception.Message == ErrorDescriptions.TheTicketIsExpired);
        }

        #endregion

        #region Happy paths

        [Fact]
        public async Task When_Requesting_Unauthorized_Access_Then_NotAuthorized_Is_Returned()
        {
            // ARRANGE
            const string ticketId = "ticket_id";
            const string clientId = "client_id";
            InitializeFakeObjects();
            var getAuthorizationActionParameter = new GetAuthorizationActionParameter
            {
                TicketId = ticketId
            };
            var claims = new List<System.Security.Claims.Claim>
            {
                new System.Security.Claims.Claim("type", "value"),
                new System.Security.Claims.Claim("client_id", clientId)
            };
            var ticket = new Ticket
            {
                ClientId = clientId,
                ExpirationDateTime = DateTime.UtcNow.AddSeconds(40)
            };
            _ticketRepositoryStub.Setup(t => t.GetTicketById(It.IsAny<string>()))
                .Returns(ticket);
            _authorizationPolicyValidatorStub.Setup(a => a.IsAuthorized(It.IsAny<Ticket>(),
                It.IsAny<string>(),
                It.IsAny<List<ClaimTokenParameter>>()))
                .Returns(Task.FromResult(new AuthorizationPolicyResult
                {
                    Type = AuthorizationPolicyResultEnum.NotAuthorized
                }));

            // ACT
            var result = await _getAuthorizationAction.Execute(getAuthorizationActionParameter, claims);

            // ASSERTS
            Assert.NotNull(result);
            Assert.True(result.AuthorizationPolicyResult == AuthorizationPolicyResultEnum.NotAuthorized);
            Assert.Null(result.Rpt);
        }

        [Fact]
        public async Task When_Requesting_Authorized_Access_Then_Rpt_Is_Returned()
        {
            // ARRANGE
            const string ticketId = "ticket_id";
            const string clientId = "client_id";
            InitializeFakeObjects();
            var getAuthorizationActionParameter = new GetAuthorizationActionParameter
            {
                TicketId = ticketId
            };
            var claims = new List<System.Security.Claims.Claim>
            {
                new System.Security.Claims.Claim("type", "value"),
                new System.Security.Claims.Claim("client_id", clientId)
            };
            var ticket = new Ticket
            {
                ClientId = clientId,
                ExpirationDateTime = DateTime.UtcNow.AddSeconds(40)
            };
            _ticketRepositoryStub.Setup(t => t.GetTicketById(It.IsAny<string>()))
                .Returns(ticket);
            _authorizationPolicyValidatorStub.Setup(a => a.IsAuthorized(It.IsAny<Ticket>(),
                It.IsAny<string>(),
                It.IsAny<List<ClaimTokenParameter>>()))
                .Returns(Task.FromResult(new AuthorizationPolicyResult
                {
                    Type = AuthorizationPolicyResultEnum.Authorized
                }));
            _repositoryExceptionHandlerStub.Setup(r => r.HandleException(It.IsAny<string>(), It.IsAny<Func<bool>>()))
                .Returns(true);

            // ACT
            var result = await _getAuthorizationAction.Execute(getAuthorizationActionParameter, claims);

            // ASSERTS
            Assert.NotNull(result);
            Assert.True(result.AuthorizationPolicyResult == AuthorizationPolicyResultEnum.Authorized);
            Assert.NotEmpty(result.Rpt);
        }

        #endregion

        private void InitializeFakeObjects()
        {
            _ticketRepositoryStub = new Mock<ITicketRepository>();
            _authorizationPolicyValidatorStub = new Mock<IAuthorizationPolicyValidator>();
           _umaServerOptionsStub = new Mock<UmaServerOptions>();
            _rptRepositoryStub = new Mock<IRptRepository>();
            _repositoryExceptionHandlerStub = new Mock<IRepositoryExceptionHelper>();
            _getAuthorizationAction = new GetAuthorizationAction(
                _ticketRepositoryStub.Object,
                _authorizationPolicyValidatorStub.Object,
                _umaServerOptionsStub.Object,
                _rptRepositoryStub.Object,
                _repositoryExceptionHandlerStub.Object);
        }
    }
}
