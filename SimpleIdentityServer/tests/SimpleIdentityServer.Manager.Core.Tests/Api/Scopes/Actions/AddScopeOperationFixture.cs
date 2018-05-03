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
using SimpleIdentityServer.Manager.Core.Api.Scopes.Actions;
using SimpleIdentityServer.Manager.Core.Errors;
using SimpleIdentityServer.Manager.Core.Exceptions;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Manager.Core.Tests.Api.Scopes.Actions
{
    public class AddScopeOperationFixture
    {
        private Mock<IScopeRepository> _scopeRepositoryStub;
        private IAddScopeOperation _addScopeOperation;

        [Fact]
        public async Task When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            await Assert.ThrowsAsync<ArgumentNullException>(() => _addScopeOperation.Execute(null));
        }

        [Fact]
        public async Task When_Scope_Already_Exists_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            const string name = "scope_name";
            var scope = new Scope();
            InitializeFakeObjects();
            _scopeRepositoryStub.Setup(s => s.GetAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(scope));

            // ACT & ASSERTS
            var ex = await Assert.ThrowsAsync<IdentityServerManagerException>(() => _addScopeOperation.Execute(new Scope
            {
                Name = name
            }));
            Assert.NotNull(ex);
            Assert.True(ex.Code == ErrorCodes.InvalidParameterCode);
            Assert.True(ex.Message == string.Format(ErrorDescriptions.TheScopeAlreadyExists, name));
        }

        [Fact]
        public async Task When_AddingScope_Then_Operation_Is_Called()
        {
            // ARRANGE
            var parameter = new Scope
            {
                Name = "scope_name"
            };
            InitializeFakeObjects();
            _scopeRepositoryStub.Setup(s => s.GetAsync(It.IsAny<string>()))
                .Returns(Task.FromResult((Scope)null));

            // ACT
            await _addScopeOperation.Execute(parameter);

            // ASSERT
            _scopeRepositoryStub.Verify(s => s.InsertAsync(parameter));
        }

        private void InitializeFakeObjects()
        {
            _scopeRepositoryStub = new Mock<IScopeRepository>();
            _addScopeOperation = new AddScopeOperation(_scopeRepositoryStub.Object);
        }
    }
}
