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
using SimpleIdentityServer.Uma.Core.Repositories;
using System;
using System.Collections.Generic;
using Xunit;

namespace SimpleIdentityServer.Uma.Core.UnitTests.Api.PolicyController
{
    public class DeleteResourcePolicyActionFixture
    {
        #region Fields

        private Mock<IPolicyRepository> _policyRepositoryStub;

        private Mock<IRepositoryExceptionHelper> _repositoryExceptionHelperStub;

        private Mock<IResourceSetRepository> _resourceSetRepositoryStub;

        private IDeleteResourcePolicyAction _deleteResourcePolicyAction;

        #endregion

        #region Exceptions

        [Fact]
        public void When_Passing_NullOrEmpty_Parameters_Then_Exceptions_Are_Throwns()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACTS & ASSERTS
            Assert.Throws<ArgumentNullException>(() => _deleteResourcePolicyAction.Execute(null, null));
            Assert.Throws<ArgumentNullException>(() => _deleteResourcePolicyAction.Execute(string.Empty, null));
            Assert.Throws<ArgumentNullException>(() => _deleteResourcePolicyAction.Execute("policy_id", null));
            Assert.Throws<ArgumentNullException>(() => _deleteResourcePolicyAction.Execute("policy_id", string.Empty));
        }

        [Fact]
        public void When_ResourceDoesntExist_Then_Exception_Is_Thrown()
        {
            const string policyId = "policy_id";
            const string resourceId = "resource_id";

            // ARRANGE
            InitializeFakeObjects();
            _repositoryExceptionHelperStub.Setup(r =>
                r.HandleException(string.Format(ErrorDescriptions.TheAuthorizationPolicyCannotBeRetrieved, policyId), It.IsAny<Func<Policy>>()))
                .Returns(() => new Policy());
            _repositoryExceptionHelperStub.Setup(r =>
                r.HandleException(string.Format(ErrorDescriptions.TheResourceSetCannotBeRetrieved, resourceId), It.IsAny<Func<ResourceSet>>()))
                .Returns(() => null);

            // ACT
            var exception = Assert.Throws<BaseUmaException>(() => _deleteResourcePolicyAction.Execute(policyId, resourceId));

            // ASSERTS
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidResourceSetId);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheResourceSetDoesntExist, resourceId));
        }

        [Fact]
        public void When_PolicyDoesntContainResource_Then_Exception_Is_Thrown()
        {
            const string policyId = "policy_id";
            const string resourceId = "invalid_resource_id";

            // ARRANGE
            InitializeFakeObjects();
            _repositoryExceptionHelperStub.Setup(r =>
                r.HandleException(string.Format(ErrorDescriptions.TheAuthorizationPolicyCannotBeRetrieved, policyId), It.IsAny<Func<Policy>>()))
                .Returns(() => new Policy
                {
                    ResourceSetIds = new List<string>
                    {
                        "resource_id"
                    }
                });
            _repositoryExceptionHelperStub.Setup(r =>
                r.HandleException(string.Format(ErrorDescriptions.TheResourceSetCannotBeRetrieved, resourceId), It.IsAny<Func<ResourceSet>>()))
                .Returns(() => new ResourceSet());

            // ACT
            var exception = Assert.Throws<BaseUmaException>(() => _deleteResourcePolicyAction.Execute(policyId, resourceId));

            // ASSERTS
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidResourceSetId);
            Assert.True(exception.Message == ErrorDescriptions.ThePolicyDoesntContainResource);
        }

        #endregion

        #region Happy Path

        [Fact]
        public void When_AuthorizationPolicyDoesntExist_Then_False_Is_Returned()
        {
            const string policyId = "policy_id";

            // ARRANGE
            InitializeFakeObjects();
            _repositoryExceptionHelperStub.Setup(r =>
                r.HandleException(string.Format(ErrorDescriptions.TheAuthorizationPolicyCannotBeRetrieved, policyId), It.IsAny<Func<Policy>>()))
                .Returns(() => null);

            // ACT
            var result = _deleteResourcePolicyAction.Execute(policyId, "resource_id");

            // ASSERT
            Assert.False(result);
        }

        [Fact]
        public void When_ResourceIsRemovedFromPolicy_Then_True_Is_Returned()
        {
            const string policyId = "policy_id";
            const string resourceId = "resource_id";

            // ARRANGE
            InitializeFakeObjects();
            _repositoryExceptionHelperStub.Setup(r =>
                r.HandleException(string.Format(ErrorDescriptions.TheAuthorizationPolicyCannotBeRetrieved, policyId), It.IsAny<Func<Policy>>()))
                .Returns(() => new Policy
                {
                    ResourceSetIds = new List<string>
                    {
                        resourceId
                    }
                });
            _repositoryExceptionHelperStub.Setup(r =>
                r.HandleException(string.Format(ErrorDescriptions.TheResourceSetCannotBeRetrieved, resourceId), It.IsAny<Func<ResourceSet>>()))
                .Returns(() => new ResourceSet());
            _policyRepositoryStub.Setup(p => p.UpdatePolicy(It.IsAny<Policy>()))
                .Returns(true);

            // ACT
            var result = _deleteResourcePolicyAction.Execute(policyId, resourceId);

            // ASSERTS
            Assert.True(result);
        }

        #endregion

        #region Private methods

        private void InitializeFakeObjects()
        {
            _policyRepositoryStub = new Mock<IPolicyRepository>();
            _repositoryExceptionHelperStub = new Mock<IRepositoryExceptionHelper>();
            _resourceSetRepositoryStub = new Mock<IResourceSetRepository>();
            _deleteResourcePolicyAction = new DeleteResourcePolicyAction(
                _policyRepositoryStub.Object,
                _repositoryExceptionHelperStub.Object,
                _resourceSetRepositoryStub.Object);
        }

        #endregion
    }
}
