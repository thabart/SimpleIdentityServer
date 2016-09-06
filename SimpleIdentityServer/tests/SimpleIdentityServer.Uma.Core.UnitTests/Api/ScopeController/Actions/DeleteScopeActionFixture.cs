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
using SimpleIdentityServer.Uma.Logging;
using System;
using Xunit;

namespace SimpleIdentityServer.Uma.Core.UnitTests.Api.ScopeController.Actions
{
    public class DeleteScopeActionFixture
    {
        private Mock<IScopeRepository> _scopeRepositoryStub;

        private Mock<IUmaServerEventSource> _umaServerEventSourceStub;

        private IDeleteScopeAction _deleteScopeAction;

        #region Exceptions

        [Fact]
        public void When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(() => _deleteScopeAction.Execute(null));
        }

        [Fact]
        public void When_UnexpectedException_Occured_In_GetScope_Then_BaseUmaException_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            _scopeRepositoryStub.Setup(s => s.GetScope(It.IsAny<string>()))
                .Callback(() =>
                {
                    throw new Exception();
                });

            // ACT & ASSERTS
            var exception = Assert.Throws<BaseUmaException>(() => _deleteScopeAction.Execute("id"));
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InternalError);
            Assert.True(exception.Message == ErrorDescriptions.TheScopeCannotBeRetrieved);
        }

        [Fact]
        public void When_UnexpectedException_Occured_In_DeleteScope_ThenBaseUmaException_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            _scopeRepositoryStub.Setup(s => s.GetScope(It.IsAny<string>()))
                .Returns(new Scope());
            _scopeRepositoryStub.Setup(s => s.DeleteScope(It.IsAny<string>()))
                .Callback(() =>
                {
                    throw new Exception();
                });

            // ACT & ASSERTS
            var exception = Assert.Throws<BaseUmaException>(() => _deleteScopeAction.Execute("id"));
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InternalError);
            Assert.True(exception.Message == ErrorDescriptions.TheScopeCannotBeRemoved);

        }

        #endregion

        #region Happy paths

        [Fact]
        public void When_Deleting_Scope_Then_True_Is_Returned()
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
            _scopeRepositoryStub.Setup(s => s.DeleteScope(It.IsAny<string>()))
                .Returns(true);

            // ACT
            var result = _deleteScopeAction.Execute(scopeId);

            // ASSERTS
            Assert.True(result);
        }

        [Fact]
        public void When_Deleting_Not_Existing_Scope_Then_False_Is_Returned()
        {
            // ARRANGE
            const string scopeId = "scope_id";
            var scope = new Scope
            {
                Id = scopeId
            };
            InitializeFakeObjects();
            _scopeRepositoryStub.Setup(s => s.GetScope(It.IsAny<string>()))
                .Returns(() => null);

            // ACT
            var result = _deleteScopeAction.Execute(scopeId);

            // ASSERTS
            Assert.False(result);
        }

        #endregion

        #region Private methods

        private void InitializeFakeObjects()
        {
            _scopeRepositoryStub = new Mock<IScopeRepository>();
            _umaServerEventSourceStub = new Mock<IUmaServerEventSource>();
            _deleteScopeAction = new DeleteScopeAction(
                _scopeRepositoryStub.Object,
                _umaServerEventSourceStub.Object);
        }

        #endregion
    }
}
