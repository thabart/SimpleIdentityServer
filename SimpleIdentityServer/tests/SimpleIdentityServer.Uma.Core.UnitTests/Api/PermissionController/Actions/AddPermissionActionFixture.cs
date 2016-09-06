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
using SimpleIdentityServer.Uma.Logging;
using System;
using System.Collections.Generic;
using Xunit;

namespace SimpleIdentityServer.Uma.Core.UnitTests.Api.PermissionController.Actions
{
    public class AddPermissionActionFixture
    {
        private Mock<IResourceSetRepository> _resourceSetRepositoryStub;

        private Mock<ITicketRepository> _ticketRepositortStub;

        private Mock<IRepositoryExceptionHelper> _repositoryExceptionHelperStub;

        private Mock<IUmaServerEventSource> _umaServerEventSourceStub;

        private UmaServerOptions _umaServerOptions;

        private IAddPermissionAction _addPermissionAction;

        #region Exceptions

        [Fact]
        public void When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(() => _addPermissionAction.Execute(null, null));
        }

        [Fact]
        public void When_Passing_No_Client_Id_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var addPermissionParameter = new AddPermissionParameter();

            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(() => _addPermissionAction.Execute(addPermissionParameter, null));
        }

        [Fact]
        public void When_RequiredParameter_ResourceSetId_Is_Not_Specified_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            const string clientId = "client_id";
            InitializeFakeObjects();
            var addPermissionParameter = new AddPermissionParameter();

            // ACT & ASSERTS
            var exception = Assert.Throws<BaseUmaException>(() => _addPermissionAction.Execute(addPermissionParameter, clientId));
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheParameterNeedsToBeSpecified, Constants.AddPermissionNames.ResourceSetId));
        }

        [Fact]
        public void When_RequiredParameter_Scopes_Is_Not_Specified_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            const string clientId = "client_id";
            InitializeFakeObjects();
            var addPermissionParameter = new AddPermissionParameter
            {
                ResourceSetId = "resource_set_id"
            };

            // ACT & ASSERTS
            var exception = Assert.Throws<BaseUmaException>(() => _addPermissionAction.Execute(addPermissionParameter, clientId));
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheParameterNeedsToBeSpecified, Constants.AddPermissionNames.Scopes));
        }

        [Fact]
        public void When_ResourceSet_Doesnt_Exist_Then_Exception_Is_Thrown()
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
            _repositoryExceptionHelperStub.Setup(r => r.HandleException<ResourceSet>(It.IsAny<string>(), It.IsAny<Func<ResourceSet>>()))
                .Returns(() => null);

            // ACT & ASSERTS
            var exception = Assert.Throws<BaseUmaException>(() => _addPermissionAction.Execute(addPermissionParameter, clientId));
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidResourceSetId);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheResourceSetDoesntExist, resourceSetId));
        }

        [Fact]
        public void When_Scope_Doesnt_Exist_Then_Exception_Is_Thrown()
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
            var resourceSet = new ResourceSet
            {
                Id = resourceSetId,
                Scopes = new List<string>
                {
                    "scope"
                }
            };
            _repositoryExceptionHelperStub.Setup(r => r.HandleException<ResourceSet>(It.IsAny<string>(), It.IsAny<Func<ResourceSet>>()))
                .Returns(resourceSet);

            // ACT & ASSERTS
            var exception = Assert.Throws<BaseUmaException>(() => _addPermissionAction.Execute(addPermissionParameter, clientId));
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidScope);
            Assert.True(exception.Message == ErrorDescriptions.TheScopeAreNotValid);
        }

        #endregion

        #region Happy path

        [Fact]
        public void When_Adding_Permission_Then_TicketId_Is_Returned()
        {           
            // ARRANGE
            const string clientId = "client_id";
            const string resourceSetId = "resource_set_id";
            const string ticketId = "ticket_id";
            InitializeFakeObjects();
            var addPermissionParameter = new AddPermissionParameter
            {
                ResourceSetId = resourceSetId,
                Scopes = new List<string>
                {
                    "scope"
                }
            };
            var resourceSet = new ResourceSet
            {
                Id = resourceSetId,
                Scopes = new List<string>
                {
                    "scope"
                }
            };
            var ticket = new Ticket
            {
                Id = ticketId
            };
            _repositoryExceptionHelperStub.Setup(r => r.HandleException(It.IsAny<string>(), It.IsAny<Func<ResourceSet>>()))
                .Returns(resourceSet);
            _umaServerOptions.TicketLifeTime = 2;
            _repositoryExceptionHelperStub.Setup(r => r.HandleException(It.IsAny<string>(), It.IsAny<Func<Ticket>>()))
                .Returns(ticket);

            // ACT
            var result = _addPermissionAction.Execute(addPermissionParameter, clientId);

            // ASSERTS
            Assert.NotNull(result);
            Assert.True(result == ticketId);
        }

        #endregion

        private void InitializeFakeObjects()
        {
            _resourceSetRepositoryStub = new Mock<IResourceSetRepository>();
            _ticketRepositortStub = new Mock<ITicketRepository>();
            _repositoryExceptionHelperStub = new Mock<IRepositoryExceptionHelper>();
            _umaServerEventSourceStub = new Mock<IUmaServerEventSource>();
            _umaServerOptions = new UmaServerOptions();
            _addPermissionAction = new AddPermissionAction(
                _resourceSetRepositoryStub.Object,
                _ticketRepositortStub.Object,
                _repositoryExceptionHelperStub.Object,
                _umaServerOptions,
                _umaServerEventSourceStub.Object);
        }
    }
}
