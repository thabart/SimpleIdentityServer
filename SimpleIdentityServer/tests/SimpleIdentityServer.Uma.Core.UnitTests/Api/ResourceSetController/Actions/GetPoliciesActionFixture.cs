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
using SimpleIdentityServer.Uma.Core.Models;
using SimpleIdentityServer.Uma.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SimpleIdentityServer.Uma.Core.UnitTests.Api.ResourceSetController.Actions
{
    public class GetPoliciesActionFixture
    {
        private Mock<IPolicyRepository> _policyRepositoryStub;

        private IGetPoliciesAction _getPoliciesAction;

        #region Exceptions

        [Fact]
        public void When_Passing_NullOrEmpty_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACTS & ASSERTS
            Assert.Throws<ArgumentNullException>(() => _getPoliciesAction.Execute(null));
            Assert.Throws<ArgumentNullException>(() => _getPoliciesAction.Execute(string.Empty));
        }

        #endregion

        #region Happy path

        [Fact]
        public void When_RetrievingPolicies_Then_Ids_Are_Returned()
        {
            // ARRANGE
            const string policyId = "policy_id";
            InitializeFakeObjects();
            _policyRepositoryStub.Setup(p => p.GetPoliciesByResourceSetId(It.IsAny<string>()))
                .Returns(new List<Policy>
                {
                    new Policy
                    {
                        Id = policyId
                    }
                });

            // ACT
            var result = _getPoliciesAction.Execute("resource_id");

            // ASSERT
            Assert.NotNull(result);
            Assert.True(result.Count == 1);
            Assert.True(result.First() == policyId);
        }

        #endregion

        #region Private methods

        private void InitializeFakeObjects()
        {
            _policyRepositoryStub = new Mock<IPolicyRepository>();
            _getPoliciesAction = new GetPoliciesAction(_policyRepositoryStub.Object);
        }

        #endregion
    }
}
