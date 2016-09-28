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
using SimpleIdentityServer.Logging;
using SimpleIdentityServer.Manager.Core.Api.Clients.Actions;
using SimpleIdentityServer.Manager.Core.Api.Manage.Actions;
using Xunit;

namespace SimpleIdentityServer.Manager.Core.Tests.Api.Manage
{
    public class ExportActionFixture
    {
        private Mock<IGetClientsAction> _getClientsActionStub;

        private Mock<IManagerEventSource> _managerEventSourceStub;

        private IExportAction _exportAction;

        [Fact]
        public void When_Settings_Are_Exported_Then_EventsAreLogged_And_Operation_Is_Called()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT
            _exportAction.Execute();

            // ASSERTS
            _managerEventSourceStub.Verify(m => m.StartToExport());
            _getClientsActionStub.Verify(g => g.Execute());
            _managerEventSourceStub.Verify(m => m.FinishToExport());
        }

        private void InitializeFakeObjects()
        {
            _getClientsActionStub = new Mock<IGetClientsAction>();
            _managerEventSourceStub = new Mock<IManagerEventSource>();
            _exportAction = new ExportAction(
                _getClientsActionStub.Object,
                _managerEventSourceStub.Object);
        }
    }
}
