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
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SimpleIdentityServer.Uma.Core.UnitTests.Api.PolicyController
{
    public class GetAuthorizationPoliciesActionFixture
    {
        private Mock<IPolicyRepository> _policyRepositoryStub;

        private Mock<IRepositoryExceptionHelper> _repositoryExceptionHelper;

        private IGetAuthorizationPoliciesAction _getAuthorizationPoliciesAction;

        [Fact]
        public void When_Getting_Authorization_Policies_Then_A_ListIds_Is_Returned()
        {
            // ARRANGE
            const string policyId = "policy_id";
            InitializeFakeObjects();
            var policies = new List<Policy>
            {
                new Policy
                {
                    Id = policyId
                }
            };
            _repositoryExceptionHelper.Setup(r => r.HandleException(ErrorDescriptions.TheAuthorizationPolicyCannotBeRetrieved,
                It.IsAny<Func<List<Policy>>>()))
                .Returns(policies);

            // ACT
            var result = _getAuthorizationPoliciesAction.Execute();

            // ASSERT
            Assert.NotNull(result);
            Assert.True(result.Count() == 1);
            Assert.True(result.First() == policyId);
        }

        private void InitializeFakeObjects()
        {
            _policyRepositoryStub = new Mock<IPolicyRepository>();
            _repositoryExceptionHelper = new Mock<IRepositoryExceptionHelper>();
            _getAuthorizationPoliciesAction = new GetAuthorizationPoliciesAction(
                _policyRepositoryStub.Object,
                _repositoryExceptionHelper.Object);
        }
    }
}
