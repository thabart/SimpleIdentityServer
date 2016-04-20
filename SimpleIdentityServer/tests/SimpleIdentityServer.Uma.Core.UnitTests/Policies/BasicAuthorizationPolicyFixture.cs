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

using SimpleIdentityServer.Uma.Core.Models;
using SimpleIdentityServer.Uma.Core.Policies;
using System;
using System.Collections.Generic;
using Xunit;

namespace SimpleIdentityServer.Uma.Core.UnitTests.Policies
{
    public class BasicAuthorizationPolicyFixture
    {
        private IBasicAuthorizationPolicy _basicAuthorizationPolicy;

        #region Exceptions

        [Fact]
        public void When_Passing_Null_Parameters_Then_Exceptions_Are_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACTS & ASSERTS
            Assert.Throws<ArgumentNullException>(() => _basicAuthorizationPolicy.Execute(null, null));
            Assert.Throws<ArgumentNullException>(() => _basicAuthorizationPolicy.Execute(new Ticket(), null));
        }

        #endregion

        #region Happy paths

        [Fact]
        public void When_Client_Is_Not_Allowed_Then_NotAuthorized_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            var ticket = new Ticket
            {
                ClientId = "invalid_client_id"
            };

            var authorizationPolicy = new Policy
            {
                ClientIdsAllowed = new List<string>
                {
                    "client_id"
                }
            };
            
            // ACT
            var result = _basicAuthorizationPolicy.Execute(ticket, authorizationPolicy);

            // ASSERT
            Assert.True(result == AuthorizationPolicyResultEnum.NotAuthorized);
        }

        [Fact]
        public void When_ResourceOwnerConsent_Is_Required_Then_RequestSubmitted_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            var ticket = new Ticket
            {
                ClientId = "client_id",
                IsAuthorizedByRo = false
            };

            var authorizationPolicy = new Policy
            {
                ClientIdsAllowed = new List<string>
                {
                    "client_id"
                },
                IsResourceOwnerConsentNeeded = true
            };

            // ACT
            var result = _basicAuthorizationPolicy.Execute(ticket, authorizationPolicy);

            // ASSERT
            Assert.True(result == AuthorizationPolicyResultEnum.RequestSubmitted);
        }

        [Fact]
        public void When_AuthorizationPassed_Then_Authorization_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            var ticket = new Ticket
            {
                ClientId = "client_id",
                IsAuthorizedByRo = true
            };

            var authorizationPolicy = new Policy
            {
                ClientIdsAllowed = new List<string>
                {
                    "client_id"
                },
                IsResourceOwnerConsentNeeded = true
            };

            // ACT
            var result = _basicAuthorizationPolicy.Execute(ticket, authorizationPolicy);

            // ASSERT
            Assert.True(result == AuthorizationPolicyResultEnum.Authorized);
        }

        #endregion

        private void InitializeFakeObjects()
        {
            _basicAuthorizationPolicy = new BasicAuthorizationPolicy();
        }
    }
}
