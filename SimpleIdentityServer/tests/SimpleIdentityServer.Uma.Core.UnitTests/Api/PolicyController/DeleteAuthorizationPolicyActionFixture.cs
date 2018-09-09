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
using SimpleIdentityServer.Uma.Core.Repositories;
using SimpleIdentityServer.Uma.Logging;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Uma.Core.UnitTests.Api.PolicyController
{
    public class DeleteAuthorizationPolicyActionFixture
    {
        private Mock<IPolicyRepository> _policyRepositoryStub;                
        private Mock<IRepositoryExceptionHelper> _repositoryExceptionHelperStub;
        private Mock<IUmaServerEventSource> _umaServerEventSourceStub;
        private IDeleteAuthorizationPolicyAction _deleteAuthorizationPolicyAction;
        
        [Fact]
        public async Task When_Passing_Empty_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            IntializeFakeObjects();

            // ACT & ASSERT
            await Assert.ThrowsAsync<ArgumentNullException>(() => _deleteAuthorizationPolicyAction.Execute(null)).ConfigureAwait(false);
        }

        [Fact]
        public async Task When_AuthorizationPolicy_Doesnt_Exist_Then_False_Is_Returned()
        {
            // ARRANGE
            const string policyId = "policy_id";
            IntializeFakeObjects();
            _repositoryExceptionHelperStub.Setup(r => r.HandleException(string.Format(ErrorDescriptions.TheAuthorizationPolicyCannotBeRetrieved, policyId),
                It.IsAny<Func<Task<Policy>>>()))
                .Returns(() => Task.FromResult((Policy)null));

            // ACT
            var isUpdated = await _deleteAuthorizationPolicyAction.Execute(policyId).ConfigureAwait(false);

            // ASSERT
            Assert.False(isUpdated);
        }

        [Fact]
        public async Task When_AuthorizationPolicy_Exists_Then_True_Is_Returned()
        {
            // ARRANGE
            const string policyId = "policy_id";
            var policy = new Policy();
            IntializeFakeObjects();
            _repositoryExceptionHelperStub.Setup(r => r.HandleException(string.Format(ErrorDescriptions.TheAuthorizationPolicyCannotBeRetrieved, policyId),
                It.IsAny<Func<Task<Policy>>>()))
                .Returns(Task.FromResult(policy));

            // ACT
            var isUpdated = await _deleteAuthorizationPolicyAction.Execute(policyId).ConfigureAwait(false);

            // ASSERT
            Assert.True(isUpdated);
        }

        private void IntializeFakeObjects()
        {
            _policyRepositoryStub = new Mock<IPolicyRepository>();
            _repositoryExceptionHelperStub = new Mock<IRepositoryExceptionHelper>();
            _umaServerEventSourceStub = new Mock<IUmaServerEventSource>();
            _deleteAuthorizationPolicyAction = new DeleteAuthorizationPolicyAction(
                _policyRepositoryStub.Object,
                _repositoryExceptionHelperStub.Object,
                _umaServerEventSourceStub.Object);
        }
    }
}
