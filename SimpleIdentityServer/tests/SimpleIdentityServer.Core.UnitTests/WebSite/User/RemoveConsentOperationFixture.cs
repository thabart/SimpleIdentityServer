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
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.WebSite.User
{
    public class RemoveConsentOperationFixture
    {
        private Mock<IConsentRepository> _consentRepositoryStub;

        private IRemoveConsentOperation _removeConsentOperation;

        #region Exceptions

        [Fact]
        public void When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT && ASSERT
            Assert.Throws<ArgumentNullException>(() => _removeConsentOperation.Execute(null));
        }

        #endregion

        #region Happy path

        [Fact]
        public void When_Deleting_Consent_Then_Boolean_Is_Returned()
        {
            // ARRANGE
            const bool isRemoved = true;
            const string consentId = "consent_id";
            InitializeFakeObjects();
            _consentRepositoryStub.Setup(c => c.DeleteConsent(It.IsAny<Models.Consent>()))
                .Returns(isRemoved);

            // ACT
            var result = _removeConsentOperation.Execute(consentId);

            // ASSERT
            Assert.True(result == isRemoved);
        }

        #endregion

        private void InitializeFakeObjects()
        {
            _consentRepositoryStub = new Mock<IConsentRepository>();
            _removeConsentOperation = new RemoveConsentOperation(_consentRepositoryStub.Object);
        }
    }
}
