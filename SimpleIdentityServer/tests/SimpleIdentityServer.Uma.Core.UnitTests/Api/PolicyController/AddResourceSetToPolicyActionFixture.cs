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
    public class AddResourceSetToPolicyActionFixture
    {
        #region Fields
        
        private Mock<IPolicyRepository> _policyRepositoryStub;

        private Mock<IResourceSetRepository> _resourceSetRepositoryStub;

        private Mock<IRepositoryExceptionHelper> _repositoryExceptionHelperStub;

        private IAddResourceSetToPolicyAction _addResourceSetAction;

        #endregion

        #region Exceptions

        [Fact]
        public void When_Passing_Null_Parameters_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(() => _addResourceSetAction.Execute(null));
        }

        [Fact]
        public void When_Passing_NoPolicyId_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERTS
            var exception = Assert.Throws<BaseUmaException>(() => _addResourceSetAction.Execute(new AddResourceSetParameter()));
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheParameterNeedsToBeSpecified, Constants.AddResourceSetParameterNames.PolicyId));
        }

        [Fact]
        public void When_Passing_NoResourceSetIds_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERTS
            var exception = Assert.Throws<BaseUmaException>(() => _addResourceSetAction.Execute(new AddResourceSetParameter
            {
                PolicyId = "policy_id"
            }));
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheParameterNeedsToBeSpecified, Constants.AddResourceSetParameterNames.ResourceSet));
        }

        [Fact]
        public void When_One_ResourceSet_Doesnt_Exist_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            const string policyId = "policy_id";
            const string resourceSetId = "resource_set_id";
            InitializeFakeObjects();
            _repositoryExceptionHelperStub.Setup(r =>
                r.HandleException(string.Format(ErrorDescriptions.TheAuthorizationPolicyCannotBeRetrieved, policyId), It.IsAny<Func<Policy>>()))
                .Returns(() => new Policy());
            _repositoryExceptionHelperStub.Setup(r =>
                r.HandleException(string.Format(ErrorDescriptions.TheResourceSetCannotBeRetrieved, resourceSetId), It.IsAny<Func<ResourceSet>>()))
                .Returns(() => null);

            // ACT & ASSERTS
            var exception = Assert.Throws<BaseUmaException>(() => _addResourceSetAction.Execute(new AddResourceSetParameter
            {
                PolicyId = policyId,
                ResourceSets = new List<string>
                {
                    resourceSetId
                }
            }));
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidResourceSetId);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheResourceSetDoesntExist, resourceSetId));
        }

        #endregion

        #region Happy paths

        [Fact]
        public void When_AuthorizationPolicy_Doesnt_Exist_Then_False_Is_Returned()
        {
            // ARRANGE
            const string policyId = "policy_id";
            InitializeFakeObjects();
            _repositoryExceptionHelperStub.Setup(r => 
                r.HandleException(string.Format(ErrorDescriptions.TheAuthorizationPolicyCannotBeRetrieved, policyId), It.IsAny<Func<Policy>>()))
                .Returns(() => null);

            // ACT
            var result = _addResourceSetAction.Execute(new AddResourceSetParameter
            {
                PolicyId = policyId,
                ResourceSets = new List<string>
                {
                    "resource_set_id"
                }
            });

            // ASSERT
            Assert.False(result);
        }

        [Fact]
        public void When_ResourceSet_Is_Inserted_Then_True_Is_Returned()
        {
            // ARRANGE
            const string policyId = "policy_id";
            const string resourceSetId = "resource_set_id";
            InitializeFakeObjects();
            _repositoryExceptionHelperStub.Setup(r =>
                r.HandleException(string.Format(ErrorDescriptions.TheAuthorizationPolicyCannotBeRetrieved, policyId), It.IsAny<Func<Policy>>()))
                .Returns(() => new Policy
                {
                    ResourceSetIds = new List<string>()
                });
            _repositoryExceptionHelperStub.Setup(r =>
                r.HandleException(string.Format(ErrorDescriptions.TheResourceSetCannotBeRetrieved, resourceSetId), It.IsAny<Func<ResourceSet>>()))
                .Returns(() => new ResourceSet());
            _repositoryExceptionHelperStub.Setup(r =>
                r.HandleException(ErrorDescriptions.ThePolicyCannotBeUpdated, It.IsAny<Func<bool>>()))
                .Returns(true);

            // ACT
            var result = _addResourceSetAction.Execute(new AddResourceSetParameter
            {
                PolicyId = policyId,
                ResourceSets = new List<string>
                {
                    resourceSetId
                }
            });

            // ASSERT
            Assert.True(result);
        }

        #endregion

        #region Private methods

        private void InitializeFakeObjects()
        {
            _policyRepositoryStub = new Mock<IPolicyRepository>();
            _resourceSetRepositoryStub = new Mock<IResourceSetRepository>();
            _repositoryExceptionHelperStub = new Mock<IRepositoryExceptionHelper>();
            _addResourceSetAction = new AddResourceSetToPolicyAction(
                _policyRepositoryStub.Object,
                _resourceSetRepositoryStub.Object,
                _repositoryExceptionHelperStub.Object);
        }

        #endregion
    }
}
