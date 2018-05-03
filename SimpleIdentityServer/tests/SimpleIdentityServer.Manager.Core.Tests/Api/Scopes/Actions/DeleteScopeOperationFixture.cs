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
using SimpleIdentityServer.Core.Common.Models;
using SimpleIdentityServer.Core.Common.Repositories;
using SimpleIdentityServer.Logging;
using SimpleIdentityServer.Manager.Core.Api.Scopes.Actions;
using SimpleIdentityServer.Manager.Core.Errors;
using SimpleIdentityServer.Manager.Core.Exceptions;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Manager.Core.Tests.Api.Scopes.Actions
{
    public class DeleteScopeOperationFixture
    {
        private Mock<IScopeRepository> _scopeRepositoryStub;
        private Mock<IManagerEventSource> _managerEventSourceStub;
        private IDeleteScopeOperation _deleteScopeOperation;

        [Fact]
        public async Task When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            await Assert.ThrowsAsync<ArgumentNullException>(() => _deleteScopeOperation.Execute(null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => _deleteScopeOperation.Execute(string.Empty));
        }

        [Fact]
        public async Task When_Passing_Not_Existing_ScopeName_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            const string scopeName = "invalid_scope";
            InitializeFakeObjects();
            _scopeRepositoryStub.Setup(c => c.GetAsync(It.IsAny<string>()))
                .Returns(Task.FromResult((Scope)null));

            // ACT & ASSERT
            var exception = await Assert.ThrowsAsync<IdentityServerManagerException>(() => _deleteScopeOperation.Execute(scopeName));
            Assert.True(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheScopeDoesntExist, scopeName));
        }

        [Fact]
        public async Task When_Deleting_ExistingScope_Then_Operation_Is_Called()
        {
            // ARRANGE
            const string scopeName = "client_id";
            var scope = new Scope
            {
                Name = scopeName
            };
            InitializeFakeObjects();
            _scopeRepositoryStub.Setup(c => c.GetAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(scope));

            // ACT
            await _deleteScopeOperation.Execute(scopeName);

            // ASSERT
            _scopeRepositoryStub.Verify(c => c.DeleteAsync(scope));
        }

        private void InitializeFakeObjects()
        {
            _scopeRepositoryStub = new Mock<IScopeRepository>();
            _managerEventSourceStub = new Mock<IManagerEventSource>();
            _deleteScopeOperation = new DeleteScopeOperation(
                _scopeRepositoryStub.Object,
                _managerEventSourceStub.Object);
        }
    }
}
