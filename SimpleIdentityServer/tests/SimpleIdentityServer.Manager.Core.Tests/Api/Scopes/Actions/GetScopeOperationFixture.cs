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
using SimpleIdentityServer.Manager.Core.Api.Scopes.Actions;
using SimpleIdentityServer.Manager.Core.Errors;
using SimpleIdentityServer.Manager.Core.Exceptions;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Manager.Core.Tests.Api.Scopes.Actions
{
    public class GetScopeOperationFixture
    {
        private Mock<IScopeRepository> _scopeRepositoryStub;
        private IGetScopeOperation _getScopeOperation;

        [Fact]
        public async Task When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            await Assert.ThrowsAsync<ArgumentNullException>(() => _getScopeOperation.Execute(null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => _getScopeOperation.Execute(string.Empty));
        }

        [Fact]
        public async Task When_Scope_Doesnt_Exist_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            const string scopeName = "invalid_scope_name";
            InitializeFakeObjects();
            _scopeRepositoryStub.Setup(s => s.GetAsync(It.IsAny<string>()))
                .Returns(Task.FromResult((Scope)null));

            // ACT & ASSERTS
            var exception = await Assert.ThrowsAsync<IdentityServerManagerException>(() => _getScopeOperation.Execute(scopeName));
            Assert.True(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheScopeDoesntExist, scopeName));
        }

        [Fact]
        public async Task When_Scope_Is_Retrieved_Then_Scope_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _scopeRepositoryStub.Setup(s => s.GetAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(new Scope()));

            // ACT
            await _getScopeOperation.Execute("scope");

            // ASSERT
            _scopeRepositoryStub.Verify(s => s.GetAsync("scope"));
        }

        private void InitializeFakeObjects()
        {
            _scopeRepositoryStub = new Mock<IScopeRepository>();
            _getScopeOperation = new GetScopeOperation(_scopeRepositoryStub.Object);
        }
    }
}
