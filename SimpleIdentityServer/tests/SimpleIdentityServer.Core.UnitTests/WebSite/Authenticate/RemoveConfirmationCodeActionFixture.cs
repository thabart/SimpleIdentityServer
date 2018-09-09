﻿#region copyright
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
using SimpleIdentityServer.Core.WebSite.Authenticate.Actions;
using SimpleIdentityServer.Store;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.WebSite.Authenticate
{
    public class RemoveConfirmationCodeActionFixture
    {
        private Mock<IConfirmationCodeStore> _confirmationCodeStoreStub;
        private IRemoveConfirmationCodeAction _removeConfirmationCodeAction;

        [Fact]
        public async Task When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            await Assert.ThrowsAsync<ArgumentNullException>(() => _removeConfirmationCodeAction.Execute(null)).ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Code_Is_Removed_Then_Operation_Is_Called()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT
            await _removeConfirmationCodeAction.Execute("code").ConfigureAwait(false);

            // ASSERT
            _confirmationCodeStoreStub.Verify(c => c.Remove("code"));
        }

        private void InitializeFakeObjects()
        {
            _confirmationCodeStoreStub = new Mock<IConfirmationCodeStore>();
            _removeConfirmationCodeAction = new RemoveConfirmationCodeAction(_confirmationCodeStoreStub.Object);
        }
    }
}
