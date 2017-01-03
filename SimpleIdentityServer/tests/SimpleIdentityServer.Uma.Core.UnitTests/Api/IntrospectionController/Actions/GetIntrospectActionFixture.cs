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
using SimpleIdentityServer.Uma.Core.Api.IntrospectionController.Actions;
using SimpleIdentityServer.Uma.Core.Errors;
using SimpleIdentityServer.Uma.Core.Exceptions;
using SimpleIdentityServer.Uma.Core.Extensions;
using SimpleIdentityServer.Uma.Core.Models;
using SimpleIdentityServer.Uma.Core.Repositories;
using SimpleIdentityServer.Uma.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Uma.Core.UnitTests.Api.IntrospectionController.Actions
{
    public class GetIntrospectActionFixture
    {
        private Mock<IRptRepository> _rptRepositoryStub;
        private Mock<ITicketRepository> _ticketRepositoryStub;
        private Mock<IUmaServerEventSource> _umaServerEventSourceStub;
        private IGetIntrospectAction _getIntrospectAction;

        [Fact]
        public async Task When_Passing_Empty_Rpt_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            await Assert.ThrowsAsync<ArgumentNullException>(() => _getIntrospectAction.Execute(string.Empty));
        }

        [Fact]
        public async Task When_Rpt_Doesnt_Exist_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            const string rpt = "invalid_rpt";
            InitializeFakeObjects();
            _rptRepositoryStub.Setup(r => r.Get(It.IsAny<string>()))
                .Returns(Task.FromResult((Rpt)null));

            // ACT & ASSERTS
            var exception = await Assert.ThrowsAsync<BaseUmaException>(() => _getIntrospectAction.Execute(rpt));
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidRpt);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheRptsDontExist, rpt));
        }

        [Fact]
        public async Task When_Ticket_Doesnt_Exist_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            const string rpt = "rpt";
            const string ticketId = "invalid_ticket_id";
            IEnumerable<Rpt> rptsInfo = new List<Rpt>
            {
                new Rpt
                {
                    TicketId = ticketId,
                    Value = rpt
                }
            };
            IEnumerable<Ticket> tickets = null;
            InitializeFakeObjects();
            _rptRepositoryStub.Setup(r => r.Get(It.IsAny<IEnumerable<string>>()))
                .Returns(Task.FromResult(rptsInfo));
            _ticketRepositoryStub.Setup(t => t.Get(It.IsAny<IEnumerable<string>>()))
                .Returns(Task.FromResult(tickets));

            // ACT & ASSERTS
            var exception = await Assert.ThrowsAsync<BaseUmaException>(() => _getIntrospectAction.Execute(rpt));
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InternalError);
            Assert.True(exception.Message == ErrorDescriptions.AtLeastOneTicketDoesntExist);
        }

        [Fact]
        public async Task When_Rpt_Is_Expired_Then_IsActive_Is_False()
        {
            // ARRANGE
            const string rpt = "rpt";
            const string ticketId = "ticket_id";
            IEnumerable<Rpt> rptsInfo = new List<Rpt>
            {
                new Rpt
                {
                    TicketId = ticketId,
                    Value = rpt,
                    ExpirationDateTime = DateTime.UtcNow.AddSeconds(30),
                    CreateDateTime = DateTime.UtcNow
                }
            };
            IEnumerable<Ticket> tickets = new List<Ticket>
            {
                new Ticket
                {
                    Id = ticketId
                }
            };
            InitializeFakeObjects();
            _rptRepositoryStub.Setup(r => r.Get(It.IsAny<IEnumerable<string>>()))
                .Returns(Task.FromResult(rptsInfo));
            _ticketRepositoryStub.Setup(t => t.Get(It.IsAny<IEnumerable<string>>()))
                .Returns(Task.FromResult(tickets));

            // ACT
            var result = await _getIntrospectAction.Execute(rpt);

            // ASSERTS
            Assert.NotNull(result);
            Assert.False(result.IsActive);
            Assert.True(result.Expiration == rptsInfo.First().ExpirationDateTime.ConvertToUnixTimestamp());
            Assert.True(result.IssuedAt == rptsInfo.First().CreateDateTime.ConvertToUnixTimestamp());
        }

        [Fact]
        public async Task When_Permission_Is_Expired_Then_IsActive_Is_False()
        {
            // ARRANGE
            const string rpt = "rpt";
            const string ticketId = "ticket_id";
            IEnumerable<Rpt> rptsInfo = new List<Rpt>
            {
                new Rpt
                {
                    TicketId = ticketId,
                    Value = rpt,
                    ExpirationDateTime = DateTime.UtcNow.AddSeconds(30),
                    CreateDateTime = DateTime.UtcNow
                }
            };
            IEnumerable<Ticket> tickets = new List<Ticket>
            {
                new Ticket
                {
                    ExpirationDateTime = DateTime.UtcNow.AddSeconds(-30),
                    Id = ticketId
                }
            };
            InitializeFakeObjects();
            _rptRepositoryStub.Setup(r => r.Get(It.IsAny<IEnumerable<string>>()))
                .Returns(Task.FromResult(rptsInfo));
            _ticketRepositoryStub.Setup(t => t.Get(It.IsAny<IEnumerable<string>>()))
                .Returns(Task.FromResult(tickets));

            // ACT
            var result = await _getIntrospectAction.Execute(rpt);

            // ASSERTS
            Assert.NotNull(result);
            Assert.False(result.IsActive);
            Assert.True(result.Expiration == rptsInfo.First().ExpirationDateTime.ConvertToUnixTimestamp());
            Assert.True(result.IssuedAt == rptsInfo.First().CreateDateTime.ConvertToUnixTimestamp());
        }

        [Fact]
        public async Task When_Rpt_Is_Valid_Then_Permissions_Are_Returned()
        {
            // ARRANGE
            const string rpt = "rpt";
            const string ticketId = "ticket_id";
            const string resourceSetId = "resource_set";
            IEnumerable<Rpt> rptInfo = new List<Rpt>
            {
                new Rpt
                {
                    TicketId = ticketId,
                    Value = rpt,
                    ExpirationDateTime = DateTime.UtcNow.AddSeconds(30),
                    CreateDateTime = DateTime.UtcNow,
                    ResourceSetId = resourceSetId
                }
            };
            IEnumerable<Ticket> ticket = new List<Ticket>
            {
                new Ticket
                {
                    Scopes = new List<string>
                    {
                        "scope",
                        "scope_2"
                    },
                    ExpirationDateTime = DateTime.UtcNow.AddSeconds(30),
                    Id = ticketId
                }
            };
            InitializeFakeObjects();
            _rptRepositoryStub.Setup(r => r.Get(It.IsAny<IEnumerable<string>>()))
                .Returns(Task.FromResult(rptInfo));
            _ticketRepositoryStub.Setup(t => t.Get(It.IsAny<IEnumerable<string>>()))
                .Returns(Task.FromResult(ticket));

            // ACT
            var result = await _getIntrospectAction.Execute(rpt);

            // ASSERTS
            Assert.NotNull(result);
            Assert.True(result.IsActive);
            Assert.True(result.Expiration == rptInfo.First().ExpirationDateTime.ConvertToUnixTimestamp());
            Assert.True(result.IssuedAt == rptInfo.First().CreateDateTime.ConvertToUnixTimestamp());
            Assert.True(result.Permissions.Count == 1);
            var permission = result.Permissions.First();
            Assert.True(permission.ResourceSetId == resourceSetId);
            Assert.True(permission.Scopes.Count() == ticket.First().Scopes.Count());
            Assert.True(permission.Expiration == ticket.First().ExpirationDateTime.ConvertToUnixTimestamp());
        }

        private void InitializeFakeObjects()
        {
            _rptRepositoryStub = new Mock<IRptRepository>();
            _ticketRepositoryStub = new Mock<ITicketRepository>();
            _umaServerEventSourceStub = new Mock<IUmaServerEventSource>();
            _getIntrospectAction = new GetIntrospectAction(
                _rptRepositoryStub.Object,
                _ticketRepositoryStub.Object,
                _umaServerEventSourceStub.Object);
        }
    }
}
