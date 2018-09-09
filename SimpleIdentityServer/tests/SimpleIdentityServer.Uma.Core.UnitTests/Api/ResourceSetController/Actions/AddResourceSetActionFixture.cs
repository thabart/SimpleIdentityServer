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
using SimpleIdentityServer.Uma.Core.Parameters;
using SimpleIdentityServer.Uma.Core.Api.ResourceSetController.Actions;
using SimpleIdentityServer.Uma.Core.Errors;
using SimpleIdentityServer.Uma.Core.Exceptions;
using SimpleIdentityServer.Uma.Core.Models;
using SimpleIdentityServer.Uma.Core.Repositories;
using SimpleIdentityServer.Uma.Core.Validators;
using System;
using System.Collections.Generic;
using Xunit;
using SimpleIdentityServer.Uma.Logging;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Uma.Core.UnitTests.Api.ResourceSetController.Actions
{
    public class AddResourceSetActionFixture
    {
        private Mock<IResourceSetRepository> _resourceSetRepositoryStub;
        private Mock<IResourceSetParameterValidator> _resourceSetParameterValidatorStub;
        private Mock<IUmaServerEventSource> _umaServerEventSourceStub;
        private IAddResourceSetAction _addResourceSetAction;

        [Fact]
        public async Task When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            await Assert.ThrowsAsync<ArgumentNullException>(() => _addResourceSetAction.Execute(null)).ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Resource_Set_Cannot_Be_Inserted_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var addResourceParameter = new AddResouceSetParameter
            {
                Name = "name",
                Scopes = new List<string> { "scope" },
                IconUri = "http://localhost",
                Uri = "http://localhost"
            };
            _resourceSetRepositoryStub.Setup(r => r.Insert(It.IsAny<ResourceSet>()))
                .Returns(() => Task.FromResult(false));

            // ACT & ASSERTS
            var exception = await Assert.ThrowsAsync<BaseUmaException>(() => _addResourceSetAction.Execute(addResourceParameter)).ConfigureAwait(false);
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InternalError);
            Assert.True(exception.Message == ErrorDescriptions.TheResourceSetCannotBeInserted);
        }

        [Fact]
        public async Task When_ResourceSet_Is_Inserted_Then_Id_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            var addResourceParameter = new AddResouceSetParameter
            {
                Name = "name",
                Scopes = new List<string> { "scope" },
                IconUri = "http://localhost",
                Uri = "http://localhost"
            };
            _resourceSetRepositoryStub.Setup(r => r.Insert(It.IsAny<ResourceSet>()))
                .Returns(Task.FromResult(true));

            // ACT
            var result = await _addResourceSetAction.Execute(addResourceParameter).ConfigureAwait(false);

            // ASSERTS
            Assert.NotNull(result);
        }

        private void InitializeFakeObjects()
        {
            _resourceSetRepositoryStub = new Mock<IResourceSetRepository>();
            _resourceSetParameterValidatorStub = new Mock<IResourceSetParameterValidator>();
            _umaServerEventSourceStub = new Mock<IUmaServerEventSource>();
            _addResourceSetAction = new AddResourceSetAction(
                _resourceSetRepositoryStub.Object,
                _resourceSetParameterValidatorStub.Object,
                _umaServerEventSourceStub.Object);
        }
    }
}
