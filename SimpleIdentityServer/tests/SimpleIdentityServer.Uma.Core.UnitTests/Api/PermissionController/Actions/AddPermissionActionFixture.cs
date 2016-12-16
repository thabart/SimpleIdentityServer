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
using SimpleIdentityServer.Uma.Core.Api.PermissionController.Actions;
using SimpleIdentityServer.Uma.Core.Errors;
using SimpleIdentityServer.Uma.Core.Exceptions;
using SimpleIdentityServer.Uma.Core.Helpers;
using SimpleIdentityServer.Uma.Core.Models;
using SimpleIdentityServer.Uma.Core.Parameters;
using SimpleIdentityServer.Uma.Core.Repositories;
using SimpleIdentityServer.Uma.Core.Services;
using SimpleIdentityServer.Uma.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Uma.Core.UnitTests.Api.PermissionController.Actions
{
    public class AddPermissionActionFixture
    {
        private Mock<IResourceSetRepository> _resourceSetRepositoryStub;
        private Mock<ITicketRepository> _ticketRepositortStub;
        private Mock<IRepositoryExceptionHelper> _repositoryExceptionHelperStub;
        private Mock<IUmaServerEventSource> _umaServerEventSourceStub;
        private Mock<IConfigurationService> _configurationServiceStub;
        private IAddPermissionAction _addPermissionAction;

        [Fact]
        public async Task When_Passing_No_Parameters_Then_Exceptions_Are_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            await Assert.ThrowsAsync<ArgumentNullException>(() => _addPermissionAction.Execute(null, (IEnumerable<AddPermissionParameter>)null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => _addPermissionAction.Execute("client_id", (IEnumerable<AddPermissionParameter>)null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => _addPermissionAction.Execute(null, (AddPermissionParameter)null));
        }

        [Fact]
        public async Task When_RequiredParameter_ResourceSetId_Is_Not_Specified_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            const string clientId = "client_id";
            InitializeFakeObjects();
            var addPermissionParameter = new AddPermissionParameter();

            // ACT & ASSERTS
            var exception = await Assert.ThrowsAsync<BaseUmaException>(() => _addPermissionAction.Execute(clientId, addPermissionParameter));
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheParameterNeedsToBeSpecified, Constants.AddPermissionNames.ResourceSetId));
        }

        [Fact]
        public async Task When_RequiredParameter_Scopes_Is_Not_Specified_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            const string clientId = "client_id";
            InitializeFakeObjects();
            var addPermissionParameter = new AddPermissionParameter
            {
                ResourceSetId = "resource_set_id"
            };

            // ACT & ASSERTS
            var exception = await Assert.ThrowsAsync<BaseUmaException>(() => _addPermissionAction.Execute(clientId, addPermissionParameter));
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheParameterNeedsToBeSpecified, Constants.AddPermissionNames.Scopes));
        }

        [Fact]
        public async Task When_ResourceSet_Doesnt_Exist_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            const string clientId = "client_id";
            const string resourceSetId = "resource_set_id";
            InitializeFakeObjects();
            var addPermissionParameter = new AddPermissionParameter
            {
                ResourceSetId = resourceSetId,
                Scopes = new List<string>
                {
                    "scope"
                }
            };
            _repositoryExceptionHelperStub.Setup(r => r.HandleException(It.IsAny<string>(), It.IsAny<Func<Task<IEnumerable<ResourceSet>>>>()))
                .Returns(Task.FromResult((IEnumerable<ResourceSet>)new List<ResourceSet>()));

            // ACT & ASSERTS
            var exception = await Assert.ThrowsAsync<BaseUmaException>(() => _addPermissionAction.Execute(clientId, addPermissionParameter));
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidResourceSetId);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheResourceSetDoesntExist, resourceSetId));
        }

        [Fact]
        public async Task When_Scope_Doesnt_Exist_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            const string clientId = "client_id";
            const string resourceSetId = "resource_set_id";
            InitializeFakeObjects();
            var addPermissionParameter = new AddPermissionParameter
            {
                ResourceSetId = resourceSetId,
                Scopes = new List<string>
                {
                    "invalid_scope"
                }
            };
            IEnumerable<ResourceSet> resources = new List<ResourceSet>
            {
                new ResourceSet
                {
                    Id = resourceSetId,
                    Scopes = new List<string>
                    {
                        "scope"
                    }
                }
            };
            _repositoryExceptionHelperStub.Setup(r => r.HandleException(It.IsAny<string>(), It.IsAny<Func<Task<IEnumerable<ResourceSet>>>>()))
                .Returns(Task.FromResult(resources));

            // ACT & ASSERTS
            var exception = await Assert.ThrowsAsync<BaseUmaException>(() => _addPermissionAction.Execute(clientId, addPermissionParameter));
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidScope);
            Assert.True(exception.Message == ErrorDescriptions.TheScopeAreNotValid);
        }

        [Fact]
        public async Task When_Adding_Permission_Then_TicketId_Is_Returned()
        {           
            // ARRANGE
            const string clientId = "client_id";
            const string resourceSetId = "resource_set_id";
            InitializeFakeObjects();
            var addPermissionParameter = new AddPermissionParameter
            {
                ResourceSetId = resourceSetId,
                Scopes = new List<string>
                {
                    "scope"
                }
            };
            IEnumerable<ResourceSet> resources = new List<ResourceSet>
            {
                new ResourceSet
                {
                    Id = resourceSetId,
                    Scopes = new List<string>
                    {
                        "scope"
                    }
                }
            };
            _repositoryExceptionHelperStub.Setup(r => r.HandleException(It.IsAny<string>(), It.IsAny<Func<Task<IEnumerable<ResourceSet>>>>()))
                .Returns(Task.FromResult(resources));
            _configurationServiceStub.Setup(c => c.GetTicketLifeTime()).Returns(Task.FromResult(2));

            // ACT
            var result = await _addPermissionAction.Execute(clientId, addPermissionParameter);

            // ASSERTS
            Assert.NotEmpty(result);
        }

        private void InitializeFakeObjects()
        {
            _resourceSetRepositoryStub = new Mock<IResourceSetRepository>();
            _ticketRepositortStub = new Mock<ITicketRepository>();
            _repositoryExceptionHelperStub = new Mock<IRepositoryExceptionHelper>();
            _umaServerEventSourceStub = new Mock<IUmaServerEventSource>();
            _configurationServiceStub = new Mock<IConfigurationService>();
            _addPermissionAction = new AddPermissionAction(
                _resourceSetRepositoryStub.Object,
                _ticketRepositortStub.Object,
                _repositoryExceptionHelperStub.Object,
                _configurationServiceStub.Object,
                _umaServerEventSourceStub.Object);
        }
    }
}
