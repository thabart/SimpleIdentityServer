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
using SimpleIdentityServer.Uma.Core.Helpers;
using SimpleIdentityServer.Uma.Core.Models;
using SimpleIdentityServer.Uma.Core.Parameters;
using SimpleIdentityServer.Uma.Core.Repositories;
using System;
using Xunit;

namespace SimpleIdentityServer.Uma.Core.UnitTests.Api.PolicyController
{
    public class UpdatePolicyActionFixture
    {
        private Mock<IPolicyRepository> _policyRepositoryStub;

        private Mock<IRepositoryExceptionHelper> _repositoryExceptionHelperStub;
        
        private IUpdatePolicyAction _updatePolicyAction;

        #region Exceptions

        [Fact]
        public void When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(() => _updatePolicyAction.Execute(null));
        }

        #endregion

        #region Happy paths
        
        [Fact]
        public void When_Authorization_Policy_Doesnt_Exist_Then_False_Is_Returned()
        {
            // ARRANGE
            var updatePolicyParameter = new UpdatePolicyParameter
            {
                PolicyId = "not_valid_policy_id"
            };
            InitializeFakeObjects();
            _repositoryExceptionHelperStub.Setup(r => r.HandleException(
                string.Format(ErrorDescriptions.TheAuthorizationPolicyCannotBeRetrieved, updatePolicyParameter.PolicyId),
                It.IsAny<Func<Policy>>())).Returns(() => null);

            // ACT
            var result = _updatePolicyAction.Execute(updatePolicyParameter);

            // ASSERT
            Assert.False(result);
        }

        [Fact]
        public void When_Authorization_Policy_Is_Updated_Then_True_Is_Returned()
        {
            // ARRANGE
            var updatePolicyParameter = new UpdatePolicyParameter
            {
                PolicyId = "valid_policy_id"
            };
            InitializeFakeObjects();
            _repositoryExceptionHelperStub.Setup(r => r.HandleException(
                string.Format(ErrorDescriptions.TheAuthorizationPolicyCannotBeRetrieved, updatePolicyParameter.PolicyId),
                It.IsAny<Func<Policy>>())).Returns(new Policy());
            _repositoryExceptionHelperStub.Setup(r => r.HandleException(
                string.Format(ErrorDescriptions.TheAuthorizationPolicyCannotBeUpdated, updatePolicyParameter.PolicyId),
                It.IsAny<Func<bool>>())).Returns(true);
            
            // ACT
            var result = _updatePolicyAction.Execute(updatePolicyParameter);

            // ASSERT
            Assert.True(result);
        }

        #endregion

        private void InitializeFakeObjects()
        {
            _policyRepositoryStub = new Mock<IPolicyRepository>();
            _repositoryExceptionHelperStub = new Mock<IRepositoryExceptionHelper>();
            _updatePolicyAction = new UpdatePolicyAction(
                _policyRepositoryStub.Object,
                _repositoryExceptionHelperStub.Object);
        }
    }
}
