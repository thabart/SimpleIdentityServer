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
using System.Collections.Generic;
using Xunit;

namespace SimpleIdentityServer.Manager.Core.Tests.Api.Scopes.Actions
{
    public class GetScopesOperationFixture
    {
        private Mock<IScopeRepository> _scopeRepositoryStub;

        private IGetScopesOperation _getScopesOperation;

        #region Happy path

        [Fact]
        public void When_Executing_Operation_Then_Operation_Is_Called()
        {
            // ARRANGE
            InitializeFakeObjects();
            _scopeRepositoryStub.Setup(c => c.GetAllScopes())
                .Returns(new List<Scope>());

            // ACT
            _getScopesOperation.Execute();

            // ASSERT
            _scopeRepositoryStub.Verify(c => c.GetAllScopes());
        }

        #endregion

        #region Private methods

        private void InitializeFakeObjects()
        {
            _scopeRepositoryStub = new Mock<IScopeRepository>();
            _getScopesOperation = new GetScopesOperation(_scopeRepositoryStub.Object);
        }

        #endregion
    }
}
