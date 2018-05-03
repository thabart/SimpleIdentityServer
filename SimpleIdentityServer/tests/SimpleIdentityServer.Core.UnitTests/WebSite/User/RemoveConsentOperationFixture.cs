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
using SimpleIdentityServer.Core.Common.Repositories;
using SimpleIdentityServer.Core.WebSite.User.Actions;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.WebSite.User
{
    public class RemoveConsentOperationFixture
    {
        private Mock<IConsentRepository> _consentRepositoryStub;
        private IRemoveConsentOperation _removeConsentOperation;

        [Fact]
        public async Task When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT && ASSERT
            await Assert.ThrowsAsync<ArgumentNullException>(() => _removeConsentOperation.Execute(null));
        }

        [Fact]
        public async Task When_Deleting_Consent_Then_Boolean_Is_Returned()
        {
            // ARRANGE
            const bool isRemoved = true;
            const string consentId = "consent_id";
            InitializeFakeObjects();
            _consentRepositoryStub.Setup(c => c.DeleteAsync(It.IsAny<Core.Common.Models.Consent>()))
                .Returns(Task.FromResult(isRemoved));

            // ACT
            var result = await _removeConsentOperation.Execute(consentId);

            // ASSERT
            Assert.True(result == isRemoved);
        }

        private void InitializeFakeObjects()
        {
            _consentRepositoryStub = new Mock<IConsentRepository>();
            _removeConsentOperation = new RemoveConsentOperation(_consentRepositoryStub.Object);
        }
    }
}
