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
using SimpleIdentityServer.Uma.Core.Errors;
using SimpleIdentityServer.Uma.Core.Exceptions;
using SimpleIdentityServer.Uma.Core.Models;
using SimpleIdentityServer.Uma.Core.Parameters;
using SimpleIdentityServer.Uma.Core.Policies;
using SimpleIdentityServer.Uma.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using Xunit;

namespace SimpleIdentityServer.Uma.Core.UnitTests.Policies
{
    public class AuthorizationPolicyValidatorFixture
    {
        private Mock<IPolicyRepository> _policyRepositoryStub;

        private Mock<IBasicAuthorizationPolicy> _basicAuthorizationPolicyStub;

        private Mock<IResourceSetRepository> _resourceSetRepositoryStub;

        private IAuthorizationPolicyValidator _authorizationPolicyValidator;

        #region Exceptions

        [Fact]
        public void When_Passing_Null_Parameters_Then_Exceptions_Are_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACTS & ASSERTS
            Assert.Throws<ArgumentNullException>(() => _authorizationPolicyValidator.IsAuthorized(null, null, null));
            Assert.Throws<ArgumentNullException>(() => _authorizationPolicyValidator.IsAuthorized(new Ticket(), null, null));
            Assert.Throws<ArgumentNullException>(() => _authorizationPolicyValidator.IsAuthorized(new Ticket(), "client_id", null));
        }

        [Fact]
        public void When_ResourceSet_Doesnt_Exist_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            var ticket = new Ticket
            {
                ResourceSetId = "resource_set_id"
            };
            var claims = new List<System.Security.Claims.Claim>
            {
                new System.Security.Claims.Claim("subject", "value")
            };
            InitializeFakeObjects();
            _resourceSetRepositoryStub.Setup(r => r.GetResourceSetById(It.IsAny<string>()))
                .Returns(() => null);

            // ACT & ASSERTS
            var exception = Assert.Throws<BaseUmaException>(() => _authorizationPolicyValidator.IsAuthorized(ticket, "client_id", claims));
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InternalError);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheResourceSetDoesntExist, ticket.ResourceSetId));
        }

        #endregion

        #region Happy paths

        [Fact]
        public void When_Policy_Doesnt_Exist_Then_Authorized_Is_Returned()
        {
            // ARRANGE
            var ticket = new Ticket();
            var claims = new List<System.Security.Claims.Claim>
            {
                new System.Security.Claims.Claim("subject", "value")
            };
            var resourceSet = new ResourceSet
            {
                AuthorizationPolicyId = "authorization_policy_id"
            };
            InitializeFakeObjects();
            _resourceSetRepositoryStub.Setup(r => r.GetResourceSetById(It.IsAny<string>()))
                .Returns(resourceSet);
            _policyRepositoryStub.Setup(p => p.GetPolicy(It.IsAny<string>()))
                .Returns(() => null);

            // ACT
            var result = _authorizationPolicyValidator.IsAuthorized(ticket, "client_id", claims);

            // ASSERT
            Assert.True(result.Type == AuthorizationPolicyResultEnum.Authorized);
        }

        [Fact]
        public void When_AuthorizationPolicy_Is_Correct_Then_Authorized_Is_Returned()
        {
            // ARRANGE
            var ticket = new Ticket();
            var claims = new List<System.Security.Claims.Claim>
            {
                new System.Security.Claims.Claim("subject", "value")
            };
            var resourceSet = new ResourceSet
            {
                AuthorizationPolicyId = "authorization_policy_id"
            };
            var policy = new Policy();
            InitializeFakeObjects();
            _resourceSetRepositoryStub.Setup(r => r.GetResourceSetById(It.IsAny<string>()))
                .Returns(resourceSet);
            _policyRepositoryStub.Setup(p => p.GetPolicy(It.IsAny<string>()))
                .Returns(policy);
            _basicAuthorizationPolicyStub.Setup(b => b.Execute(It.IsAny<Ticket>(), It.IsAny<Policy>(), It.IsAny<List<ClaimTokenParameter>>()))
                .Returns(new AuthorizationPolicyResult
                {
                    Type = AuthorizationPolicyResultEnum.Authorized
                });

            // ACT
            var result = _authorizationPolicyValidator.IsAuthorized(ticket, "client_id", claims);

            // ASSERT
            Assert.True(result.Type == AuthorizationPolicyResultEnum.Authorized);
        }

        #endregion

        private void InitializeFakeObjects()
        {
            _policyRepositoryStub = new Mock<IPolicyRepository>();
            _basicAuthorizationPolicyStub = new Mock<IBasicAuthorizationPolicy>();
            _resourceSetRepositoryStub = new Mock<IResourceSetRepository>();
            _authorizationPolicyValidator = new AuthorizationPolicyValidator(
                _policyRepositoryStub.Object,
                _basicAuthorizationPolicyStub.Object,
                _resourceSetRepositoryStub.Object);
        }
    }
}
