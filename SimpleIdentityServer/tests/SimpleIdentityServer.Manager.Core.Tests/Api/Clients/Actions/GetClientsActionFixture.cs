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
using SimpleIdentityServer.Core.Common.Repositories;
using SimpleIdentityServer.Manager.Core.Api.Clients.Actions;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Manager.Core.Tests.Api.Clients.Actions
{
    public class GetClientsActionFixture
    {
        private Mock<IClientRepository> _clientRepositoryStub;
        private IGetClientsAction _getClientsAction;

        [Fact]
        public async Task When_Executing_Operation_Then_Operation_Is_Called()
        {
            // ARRANGE
            IEnumerable<SimpleIdentityServer.Core.Common.Models.Client> clients = new List<SimpleIdentityServer.Core.Common.Models.Client>();
            InitializeFakeObjects();
            _clientRepositoryStub.Setup(c => c.GetAllAsync())
                .Returns(Task.FromResult(clients));

            // ACT
            await _getClientsAction.Execute();

            // ASSERT
            _clientRepositoryStub.Verify(c => c.GetAllAsync());
        }
        
        private void InitializeFakeObjects()
        {
            _clientRepositoryStub = new Mock<IClientRepository>();
            _getClientsAction = new GetClientsAction(_clientRepositoryStub.Object);
        }
    }
}
