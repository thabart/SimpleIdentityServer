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
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SimpleIdentityServer.Uma.Core.UnitTests.Api.ScopeController.Actions
{
    public class GetScopesActionFixture
    {
        private Mock<IScopeRepository> _scopeRepositoryStub;

        private IGetScopesAction _getScopesAction;

        #region Exceptions

        [Fact]
        public void When_UnexpectedException_Is_Thrown_Then_BaseUmaException_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            _scopeRepositoryStub.Setup(s => s.GetAll())
                .Callback(() =>
                {
                    throw new Exception();
                });

            // ACT & ASSERTS
            var exception = Assert.Throws<BaseUmaException>(() => _getScopesAction.Execute());
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InternalError);
            Assert.True(exception.Message == ErrorDescriptions.TheScopesCannotBeRetrieved);
        }

        #endregion

        #region Happy paths

        [Fact]
        public void When_Retrieving_Scopes_Then_Ids_Are_Returned()
        {
            // ARRANGE
            const string scopeId = "scope_id";
            var scope = new Scope
            {
                Id = scopeId
            };
            InitializeFakeObjects();
            _scopeRepositoryStub.Setup(s => s.GetAll())
                .Returns(new List<Scope> { scope });

            // ACT
            var result = _getScopesAction.Execute();

            // ASSERTS
            Assert.NotNull(result);
            Assert.True(result.Count() == 1);
            Assert.True(result.First() == scopeId);
        }

        #endregion

        #region Private methods

        private void InitializeFakeObjects()
        {
            _scopeRepositoryStub = new Mock<IScopeRepository>();
            _getScopesAction = new GetScopesAction(_scopeRepositoryStub.Object);
        }

        #endregion
    }
}
