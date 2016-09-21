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
using SimpleIdentityServer.Core.WebSite.Authenticate.Actions;
using System;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.WebSite.Authenticate
{
    public class RemoveConfirmationCodeActionFixture
    {
        private Mock<IConfirmationCodeRepository> _confirmationCodeRepositoryStub;

        private IRemoveConfirmationCodeAction _removeConfirmationCodeAction;

        [Fact]
        public void When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(() => _removeConfirmationCodeAction.Execute(null));
        }

        [Fact]
        public void When_Code_Is_Removed_Then_Operation_Is_Called()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT
            _removeConfirmationCodeAction.Execute("code");

            // ASSERT
            _confirmationCodeRepositoryStub.Verify(c => c.Remove("code"));
        }

        private void InitializeFakeObjects()
        {
            _confirmationCodeRepositoryStub = new Mock<IConfirmationCodeRepository>();
            _removeConfirmationCodeAction = new RemoveConfirmationCodeAction(_confirmationCodeRepositoryStub.Object);
        }
    }
}
