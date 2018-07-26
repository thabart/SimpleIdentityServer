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
using SimpleIdentityServer.Uma.Core.Parameters;
using SimpleIdentityServer.Uma.Core.Repositories;
using SimpleIdentityServer.Uma.Core.Validators;
using SimpleIdentityServer.Uma.Logging;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Uma.Core.UnitTests.Api.ResourceSetController.Actions
{
    public class UpdateResourceSetActionFixture
    {
        private Mock<IResourceSetRepository> _resourceSetRepositoryStub;
        private Mock<IResourceSetParameterValidator> _resourceSetParameterValidator;
        private Mock<IUmaServerEventSource> _umaServerEventSourceStub;
        private IUpdateResourceSetAction _updateResourceSetAction;

        [Fact]
        public async Task When_Passing_No_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            await Assert.ThrowsAsync<ArgumentNullException>(() => _updateResourceSetAction.Execute(null));
        }

        [Fact]
        public async Task When_ResourceSet_Cannot_Be_Updated_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string id = "id";
            var udpateResourceSetParameter = new UpdateResourceSetParameter
            {
                Id = id
            };
            var resourceSet = new ResourceSet
            {
                Id = id
            };
            _resourceSetRepositoryStub.Setup(r => r.Get(It.IsAny<string>()))
                .Returns(Task.FromResult(resourceSet));
            _resourceSetRepositoryStub.Setup(r => r.Update(It.IsAny<ResourceSet>()))
                .Returns(() => Task.FromResult(false));

            // ACT & ASSERTS
            var exception = await Assert.ThrowsAsync<BaseUmaException>(() => _updateResourceSetAction.Execute(udpateResourceSetParameter));
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InternalError);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheResourceSetCannotBeUpdated, udpateResourceSetParameter.Id));
        }

        [Fact]
        public async Task When_A_ResourceSet_Is_Updated_Then_True_Is_Returned()
        {
            // ARRANGE
            const string id = "id";
            InitializeFakeObjects();
            var udpateResourceSetParameter = new UpdateResourceSetParameter
            {
                Id = id
            };
            var resourceSet = new ResourceSet
            {
                Id = id
            };
            _resourceSetRepositoryStub.Setup(r => r.Get(It.IsAny<string>()))
                .Returns(Task.FromResult(resourceSet));
            _resourceSetRepositoryStub.Setup(r => r.Update(It.IsAny<ResourceSet>()))
                .Returns(Task.FromResult(true));

            // ACT
            var result = await _updateResourceSetAction.Execute(udpateResourceSetParameter);

            // ASSERTS
            Assert.NotNull(result);
            Assert.True(result);

        }

        private void InitializeFakeObjects()
        {
            _resourceSetRepositoryStub = new Mock<IResourceSetRepository>();
            _resourceSetParameterValidator = new Mock<IResourceSetParameterValidator>();
            _umaServerEventSourceStub = new Mock<IUmaServerEventSource>();
            _updateResourceSetAction = new UpdateResourceSetAction(
                _resourceSetRepositoryStub.Object,
                _resourceSetParameterValidator.Object,
                _umaServerEventSourceStub.Object);
        }
    }
}
