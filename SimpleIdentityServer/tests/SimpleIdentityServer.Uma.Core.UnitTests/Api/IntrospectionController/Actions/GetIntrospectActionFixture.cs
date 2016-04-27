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
