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
using SimpleIdentityServer.Logging;
using SimpleIdentityServer.Manager.Core.Api.Manage.Actions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Manager.Core.Tests.Api.Manage
{
    public class ImportActionFixture
    {
        private Mock<IClientRepository> _clientRepositoryStub;
        private Mock<IManagerEventSource> _managerEventSourceStub;
        private IImportAction _importAction;

        [Fact]
        public async Task When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            await Assert.ThrowsAsync<ArgumentNullException>(() => _importAction.Execute(null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => _importAction.Execute(new Parameters.ImportParameter()));
        }

        [Fact]
        public async Task When_Clients_Cannot_Be_Removed_Then_False_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _clientRepositoryStub.Setup(m => m.RemoveAllAsync())
                .Returns(Task.FromResult(false));

            // ACT
            var result = await _importAction.Execute(new Parameters.ImportParameter
            {
                Clients = new List<SimpleIdentityServer.Core.Common.Models.Client>()
            });

            // ASSERTS
            _managerEventSourceStub.Verify(m => m.StartToImport());
            Assert.False(result);
        }

        [Fact]
        public async Task When_OneClient_Cannot_Be_Inserted_Then_Error_Is_Logged()
        {
            // ARRANGE
            InitializeFakeObjects();
            _clientRepositoryStub.Setup(m => m.RemoveAllAsync())
                .Returns(Task.FromResult(true));
            _clientRepositoryStub.Setup(c => c.InsertAsync(It.IsAny<SimpleIdentityServer.Core.Common.Models.Client>()))
                .Returns(Task.FromResult(false))
                .Callback<SimpleIdentityServer.Core.Common.Models.Client>((c) =>
                {
                    throw new Exception("exception");
                });

            // ACT
            var result = await _importAction.Execute(new Parameters.ImportParameter
            {
                Clients = new List<SimpleIdentityServer.Core.Common.Models.Client>
                {
                    new SimpleIdentityServer.Core.Common.Models.Client
                    {
                        ClientName = "client_name"
                    }
                }
            });

            // ASSERTS
            _managerEventSourceStub.Verify(m => m.StartToImport());
            _managerEventSourceStub.Verify(m => m.RemoveAllClients());
            _managerEventSourceStub.Verify(m => m.Failure(It.IsAny<Exception>()));
            Assert.True(result);
        }

        [Fact]
        public async Task When_Import_Is_Finished_Then_True_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _clientRepositoryStub.Setup(m => m.RemoveAllAsync())
                .Returns(Task.FromResult(true));

            // ACT
            var result = await _importAction.Execute(new Parameters.ImportParameter
            {
                Clients = new List<SimpleIdentityServer.Core.Common.Models.Client>
                {
                    new SimpleIdentityServer.Core.Common.Models.Client
                    {
                        ClientName = "client_name"
                    }
                }
            });

            // ASSERT
            _managerEventSourceStub.Verify(m => m.StartToImport());
            _managerEventSourceStub.Verify(m => m.RemoveAllClients());
            _managerEventSourceStub.Verify(m => m.FinishToImport());
            Assert.True(result);
        }

        private void InitializeFakeObjects()
        {
            _clientRepositoryStub = new Mock<IClientRepository>();
            _managerEventSourceStub = new Mock<IManagerEventSource>();
            _importAction = new ImportAction(
                _clientRepositoryStub.Object,
                _managerEventSourceStub.Object);
        }
    }
}
