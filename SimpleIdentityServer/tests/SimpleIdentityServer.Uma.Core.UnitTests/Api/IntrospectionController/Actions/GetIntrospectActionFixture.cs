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
using SimpleIdentityServer.Uma.Core.Models;
using SimpleIdentityServer.Uma.Core.Repositories;
using System;
using Xunit;

namespace SimpleIdentityServer.Uma.Core.UnitTests.Api.IntrospectionController.Actions
{
    public class GetIntrospectActionFixture
    {
        private Mock<IRptRepository> _rptRepositoryStub;

        private Mock<ITicketRepository> _ticketRepositoryStub;

        private IGetIntrospectAction _getIntrospectAction;

        #region Exceptions

        [Fact]
        public void When_Passing_Empty_Rpt_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(() => _getIntrospectAction.Execute(null));
        }

        [Fact]
        public void When_Rpt_Doesnt_Exist_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            const string rpt = "invalid_rpt";
            InitializeFakeObjects();
            _rptRepositoryStub.Setup(r => r.GetRpt(It.IsAny<string>()))
                .Returns(() => null);

            // ACT & ASSERTS
            var exception = Assert.Throws<BaseUmaException>(() => _getIntrospectAction.Execute(rpt));
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidRpt);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheRptDoesntExist, rpt));
        }

        [Fact]
        public void When_Ticket_Doesnt_Exist_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            const string rpt = "rpt";
            const string ticketId = "invalid_ticket_id";
            var rptInfo = new Rpt
            {
                TicketId = ticketId,
                Value = rpt
            };
            InitializeFakeObjects();
            _rptRepositoryStub.Setup(r => r.GetRpt(It.IsAny<string>()))
                .Returns(rptInfo);
            _ticketRepositoryStub.Setup(t => t.GetTicketById(It.IsAny<string>()))
                .Returns(() => null);

            // ACT & ASSERTS
            var exception = Assert.Throws<BaseUmaException>(() => _getIntrospectAction.Execute(rpt));
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InternalError);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheTicketDoesntExist, ticketId));
        }

        #endregion

        private void InitializeFakeObjects()
        {
            _rptRepositoryStub = new Mock<IRptRepository>();
            _ticketRepositoryStub = new Mock<ITicketRepository>();
            _getIntrospectAction = new GetIntrospectAction(
                _rptRepositoryStub.Object,
                _ticketRepositoryStub.Object);
        }
    }
}
