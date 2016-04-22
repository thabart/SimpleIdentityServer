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
using SimpleIdentityServer.Uma.Core.Api.PolicyController.Actions;
using SimpleIdentityServer.Uma.Core.Errors;
using SimpleIdentityServer.Uma.Core.Exceptions;
using SimpleIdentityServer.Uma.Core.Helpers;
using SimpleIdentityServer.Uma.Core.Models;
using SimpleIdentityServer.Uma.Core.Parameters;
using SimpleIdentityServer.Uma.Core.Repositories;
using System;
using System.Collections.Generic;
using Xunit;

namespace SimpleIdentityServer.Uma.Core.UnitTests.Api.PolicyController
{
    public class AddAuthorizationPolicyActionFixture
    {
        private Mock<IPolicyRepository> _policyRepositoryStub;

        private Mock<IResourceSetRepository> _resourceSetRepositoryStub;

        private Mock<IRepositoryExceptionHelper> _repositoryExceptionHelper;

        private IAddAuthorizationPolicyAction _addAuthorizationPolicyAction;

        #region Exceptions

        [Fact]
        public void When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(() => _addAuthorizationPolicyAction.Execute(null));
        }

        [Fact]
        public void When_Passing_Empty_ResourceSetId_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var addPolicyParameter = new AddPolicyParameter();

            // ACT & ASSERTS
            var exception = Assert.Throws<BaseUmaException>(() => _addAuthorizationPolicyAction.Execute(addPolicyParameter));
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheParameterNeedsToBeSpecified, Constants.AddPolicyParameterNames.ResourceSetIds));
        }

        [Fact]
        public void When_Passing_EmptyScript_For_Custom_Policy_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string resourceSetId = "resource_set_id";
            var addPolicyParameter = new AddPolicyParameter
            {
                ResourceSetIds = new List<string>
                {
                    resourceSetId
                },
                IsCustom = true
            };
            var resourceSet = new ResourceSet
            {
                Id = resourceSetId
            };
            _repositoryExceptionHelper.Setup(r => r.HandleException(string.Format(ErrorDescriptions.TheResourceSetCannotBeRetrieved, resourceSetId), It.IsAny<Func<ResourceSet>>()))
                .Returns(resourceSet);

            // ACT & ASSERTS
            var exception = Assert.Throws<BaseUmaException>(() => _addAuthorizationPolicyAction.Execute(addPolicyParameter));
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheParameterNeedsToBeSpecified, Constants.AddPolicyParameterNames.Script));
        }

        [Fact]
        public void When_Passing_No_Scopes_For_Generic_Policy_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string resourceSetId = "resource_set_id";
            var addPolicyParameter = new AddPolicyParameter
            {
                ResourceSetIds = new List<string>
                {
                    resourceSetId
                },
                IsCustom = false
            };

            // ACT & ASSERTS
            var exception = Assert.Throws<BaseUmaException>(() => _addAuthorizationPolicyAction.Execute(addPolicyParameter));
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheParameterNeedsToBeSpecified, Constants.AddPolicyParameterNames.Scopes));
        }

        [Fact]
        public void When_Passing_No_ClientIds_For_Generic_Policy_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string resourceSetId = "resource_set_id";
            var addPolicyParameter = new AddPolicyParameter
            {
                ResourceSetIds = new List<string>
                {
                    resourceSetId
                },
                IsCustom = false,
                Scopes = new List<string>
                {
                    "invalid_scope"
                }
            };

            // ACT & ASSERTS
            var exception = Assert.Throws<BaseUmaException>(() => _addAuthorizationPolicyAction.Execute(addPolicyParameter));
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheParameterNeedsToBeSpecified, Constants.AddPolicyParameterNames.ClientIdsAllowed));
        }
        
        [Fact]
        public void When_ResourceSetId_Doesnt_Exist_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string resourceSetId = "resource_set_id";
            var addPolicyParameter = new AddPolicyParameter
            {
                ResourceSetIds = new List<string>
                {
                    resourceSetId
                },
                IsCustom = false,
                Scopes = new List<string>
                {
                    "invalid_scope"
                },
                ClientIdsAllowed = new List<string>
                {
                    "client_id"
                }
            };
            _repositoryExceptionHelper.Setup(r => r.HandleException(string.Format(ErrorDescriptions.TheResourceSetCannotBeRetrieved, resourceSetId), It.IsAny<Func<ResourceSet>>()))
                .Returns(() => null);

            // ACT & ASSERTS
            var exception = Assert.Throws<BaseUmaException>(() => _addAuthorizationPolicyAction.Execute(addPolicyParameter));
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidResourceSetId);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheResourceSetDoesntExist, resourceSetId));
        }

        [Fact]
        public void When_Resource_Set_Is_Not_Valid_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string resourceSetId = "resource_set_id";
            var addPolicyParameter = new AddPolicyParameter
            {
                ResourceSetIds = new List<string>
                {
                    resourceSetId
                },
                IsCustom = false,
                Scopes = new List<string>
                {
                    "invalid_scope"
                },
                ClientIdsAllowed = new List<string>
                {
                    "client_id"
                }
            };
            var resourceSet = new ResourceSet
            {
                Scopes = new List<string>
                {
                    "scope"
                }
            };
            _repositoryExceptionHelper.Setup(r => r.HandleException(string.Format(ErrorDescriptions.TheResourceSetCannotBeRetrieved, resourceSetId), It.IsAny<Func<ResourceSet>>()))
                .Returns(resourceSet);

            // ACT & ASSERTS
            var exception = Assert.Throws<BaseUmaException>(() => _addAuthorizationPolicyAction.Execute(addPolicyParameter));
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidScope);
            Assert.True(exception.Message == ErrorDescriptions.OneOrMoreScopesDontBelongToAResourceSet);
        }

        #endregion
        
        #region Happy path

        [Fact]
        public void When_Adding_AuthorizationPolicy_Then_Id_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string resourceSetId = "resource_set_id";
            var addPolicyParameter = new AddPolicyParameter
            {
                ResourceSetIds = new List<string>
                {
                    resourceSetId
                },
                IsCustom = false,
                Scopes = new List<string>
                {
                    "scope"
                },
                ClientIdsAllowed = new List<string>
                {
                    "client_id"
                }
            };
            var resourceSet = new ResourceSet
            {
                Scopes = new List<string>
                {
                    "scope"
                }
            };
            _repositoryExceptionHelper.Setup(r => r.HandleException(string.Format(ErrorDescriptions.TheResourceSetCannotBeRetrieved, resourceSetId), It.IsAny<Func<ResourceSet>>()))
                .Returns(resourceSet);

            // ACT
            var result = _addAuthorizationPolicyAction.Execute(addPolicyParameter);

            // ASSERTS
            Assert.NotNull(result);
        }

        #endregion

        private void InitializeFakeObjects()
        {
            _policyRepositoryStub = new Mock<IPolicyRepository>();
            _resourceSetRepositoryStub = new Mock<IResourceSetRepository>();
            _repositoryExceptionHelper = new Mock<IRepositoryExceptionHelper>();
            _addAuthorizationPolicyAction = new AddAuthorizationPolicyAction(
                _policyRepositoryStub.Object,
                _resourceSetRepositoryStub.Object,
                _repositoryExceptionHelper.Object);
        }
    }
}
