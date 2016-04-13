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
using SimpleIdentityServer.Uma.Core.Api.ScopeController.Actions;
using SimpleIdentityServer.Uma.Core.Errors;
using SimpleIdentityServer.Uma.Core.Exceptions;
using SimpleIdentityServer.Uma.Core.Models;
using SimpleIdentityServer.Uma.Core.Repositories;
using System;
using Xunit;

namespace SimpleIdentityServer.Uma.Core.UnitTests.Api.ScopeController.Actions
{
    public class GetScopeActionFixture
    {
        private Mock<IScopeRepository> _scopeRepositoryStub;

        private IGetScopeAction _getScopeAction;

        #region Exceptions

        [Fact]
        public void When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(() => _getScopeAction.Execute(null));
        }

        [Fact]
        public void When_UnexpectedException_Is_Thrown_Then_BaseUmaException_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            _scopeRepositoryStub.Setup(s => s.GetScope(It.IsAny<string>()))
                .Callback(() =>
                {
                    throw new Exception();
                });

            // ACT & ASSERTS
            var exception = Assert.Throws<BaseUmaException>(() => _getScopeAction.Execute("id"));
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InternalError);
            Assert.True(exception.Message == ErrorDescriptions.TheScopeCannotBeRetrieved);
        }

        #endregion

        #region Happy paths

        [Fact]
        public void When_Retrieving_Scope_Then_Scope_Is_Returned()
        {
            // ARRANGE
            const string scopeId = "scope_id";
            var scope = new Scope
            {
                Id = scopeId
            };
            InitializeFakeObjects();
            _scopeRepositoryStub.Setup(s => s.GetScope(It.IsAny<string>()))
                .Returns(scope);

            // ACT
            var result = _getScopeAction.Execute(scopeId);

            // ASSERTS
            Assert.NotNull(result);
            Assert.True(result.Id == scopeId);
        }

        #endregion

        #region Private methods

        private void InitializeFakeObjects()
        {
            _scopeRepositoryStub = new Mock<IScopeRepository>();
            _getScopeAction = new GetScopeAction(_scopeRepositoryStub.Object);
        }

        #endregion
    }
}
