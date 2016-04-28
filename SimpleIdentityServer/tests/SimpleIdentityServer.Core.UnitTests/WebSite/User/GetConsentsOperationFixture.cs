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
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Core.WebSite.User.Actions;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.WebSite.User
{
    public class GetConsentsOperationFixture
    {
        private Mock<IConsentRepository> _consentRepositoryStub;

        private IGetConsentsOperation _getConsentsOperation;

        #region Exceptions

        [Fact]
        public void When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(() => _getConsentsOperation.Execute(null));
        }

        #endregion

        #region Happy paths

        [Fact]
        public void When_Getting_Consents_A_List_Is_Returned()
        {
            // ARRANGE
            const string subject = "subject";
            InitializeFakeObjects();
            var claims = new List<Claim>
            {
                new Claim(Jwt.Constants.StandardResourceOwnerClaimNames.Subject, subject)
            };
            var consents = new List<Models.Consent>
            {
                new Models.Consent
                {
                    Id = "consent_id"
                }
            };
            var claimsIdentity = new ClaimsIdentity(claims);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            _consentRepositoryStub.Setup(c => c.GetConsentsForGivenUser(subject))
                .Returns(consents);

            // ACT
            var result = _getConsentsOperation.Execute(claimsPrincipal);

            // ASSERTS
            Assert.NotNull(result);
            Assert.True(result == consents);
        }

        #endregion

        private void InitializeFakeObjects()
        {
            _consentRepositoryStub = new Mock<IConsentRepository>();
            _getConsentsOperation = new GetConsentsOperation(_consentRepositoryStub.Object);
        }
    }
}
