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
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Manager.Core.Tests.Api.Scopes.Actions
{
    public class GetScopesOperationFixture
    {
        private Mock<IScopeRepository> _scopeRepositoryStub;
        private IGetScopesOperation _getScopesOperation;
        
        [Fact]
        public async Task When_Executing_Operation_Then_Operation_Is_Called()
        {
            // ARRANGE
            ICollection<Scope> scopes = new List<Scope>();
            InitializeFakeObjects();
            _scopeRepositoryStub.Setup(c => c.GetAllAsync())
                .Returns(Task.FromResult(scopes));

            // ACT
            await _getScopesOperation.Execute();

            // ASSERT
            _scopeRepositoryStub.Verify(c => c.GetAllAsync());
        }

        private void InitializeFakeObjects()
        {
            _scopeRepositoryStub = new Mock<IScopeRepository>();
            _getScopesOperation = new GetScopesOperation(_scopeRepositoryStub.Object);
        }
    }
}
