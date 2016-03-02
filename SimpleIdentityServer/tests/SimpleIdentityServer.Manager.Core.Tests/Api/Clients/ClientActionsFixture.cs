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
using SimpleIdentityServer.Manager.Core.Api.Clients;
using SimpleIdentityServer.Manager.Core.Api.Clients.Actions;
using Xunit;

namespace SimpleIdentityServer.Manager.Core.Tests.Api.Clients
{
    public class ClientActionsFixture
    {
        private Mock<IGetClientsAction> _getClientsActionStub;

        private Mock<IGetClientAction> _getClientActionStub;

        private Mock<IRemoveClientAction> _removeClientActionStub;

        private IClientActions _clientActions;

        #region Happy path

        [Fact]
        public void When_Executing_GetClients_Then_Operation_Is_Called()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT
            _clientActions.GetClients();

            // ASSERT
            _getClientsActionStub.Verify(g => g.Execute());
        }

        [Fact]
        public void When_Executing_GetClient_Then_Operation_Is_Called()
        {
            // ARRANGE
            const string clientId = "clientId";
            InitializeFakeObjects();

            // ACT
            _clientActions.GetClient(clientId);

            // ASSERT
            _getClientActionStub.Verify(g => g.Execute(clientId));
        }

        [Fact]
        public void When_Executing_DeleteClient_Then_Operation_Is_Called()
        {
            // ARRANGE
            const string clientId = "clientId";
            InitializeFakeObjects();

            // ACT
            _clientActions.DeleteClient(clientId);

            // ASSERT
            _removeClientActionStub.Verify(g => g.Execute(clientId));
        }

        #endregion

        private void InitializeFakeObjects()
        {
            _getClientsActionStub = new Mock<IGetClientsAction>();
            _getClientActionStub = new Mock<IGetClientAction>();
            _removeClientActionStub = new Mock<IRemoveClientAction>();
            _clientActions = new ClientActions(_getClientsActionStub.Object,
                _getClientActionStub.Object,
                _removeClientActionStub.Object);
        }
    }
}
