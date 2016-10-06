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
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Manager.Core.Api.Clients.Actions;
using System.Collections.Generic;
using Xunit;

namespace SimpleIdentityServer.Manager.Core.Tests.Api.Clients.Actions
{
    public class GetClientsActionFixture
    {
        private Mock<IClientRepository> _clientRepositoryStub;

        private IGetClientsAction _getClientsAction;

        #region Happy path

        [Fact]
        public void When_Executing_Operation_Then_Operation_Is_Called()
        {
            // ARRANGE
            InitializeFakeObjects();
            _clientRepositoryStub.Setup(c => c.GetAll())
                .Returns(new List<SimpleIdentityServer.Core.Models.Client>());

            // ACT
            _getClientsAction.Execute();

            // ASSERT
            _clientRepositoryStub.Verify(c => c.GetAll());
        }

        #endregion

        private void InitializeFakeObjects()
        {
            _clientRepositoryStub = new Mock<IClientRepository>();
            _getClientsAction = new GetClientsAction(_clientRepositoryStub.Object);
        }
    }
}
