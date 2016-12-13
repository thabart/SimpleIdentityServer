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
using SimpleIdentityServer.Uma.Core.Api.ResourceSetController.Actions;
using SimpleIdentityServer.Uma.Core.Errors;
using SimpleIdentityServer.Uma.Core.Exceptions;
using SimpleIdentityServer.Uma.Core.Models;
using SimpleIdentityServer.Uma.Core.Repositories;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Uma.Core.UnitTests.Api.ResourceSetController.Actions
{
    public class GetResourceSetActionFixture
    {
        private Mock<IResourceSetRepository> _resourceSetRepositoryStub;
        private IGetResourceSetAction _getResourceSetAction;

        [Fact]
        public async Task When_Passing_Null_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT && ASSERT
            await Assert.ThrowsAsync<ArgumentNullException>(() => _getResourceSetAction.Execute(null));
        }

        [Fact]
        public async Task When_Execute_Operation_Then_Resource_Set_Is_Returned()
        {
            // ARRANGE
            var resourceSet = new ResourceSet
            {
                Id = "id"
            };
            InitializeFakeObjects();
            _resourceSetRepositoryStub.Setup(r => r.Get(It.IsAny<string>()))
                .Returns(Task.FromResult(resourceSet));

            // ACT
            var result = await _getResourceSetAction.Execute(resourceSet.Id);
        
            // ASSERTS
            Assert.NotNull(result);
            Assert.True(result.Id == resourceSet.Id);
        }

        private void InitializeFakeObjects()
        {
            _resourceSetRepositoryStub = new Mock<IResourceSetRepository>();
            _getResourceSetAction = new GetResourceSetAction(_resourceSetRepositoryStub.Object);
        }
    }
}
