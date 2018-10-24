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

using SimpleIdentityServer.Core.Services;
using System;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.TwoFactors
{
    public class TwoFactorAuthenticationHandlerFixture
    {
        private ITwoFactorAuthenticationHandler _twoFactorAuthenticationHandler;

        [Fact]
        public void When_Passing_Null_Parameter_To_SendCode_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACTS & ASSERTS
            Assert.ThrowsAsync<ArgumentNullException>(() => _twoFactorAuthenticationHandler.SendCode(null, null, null));
            Assert.ThrowsAsync<ArgumentNullException>(() => _twoFactorAuthenticationHandler.SendCode(string.Empty, null, null));
            Assert.ThrowsAsync<ArgumentNullException>(() => _twoFactorAuthenticationHandler.SendCode("code", null, null));
            Assert.ThrowsAsync<ArgumentNullException>(() => _twoFactorAuthenticationHandler.SendCode("code", string.Empty, null));
            Assert.ThrowsAsync<ArgumentNullException>(() => _twoFactorAuthenticationHandler.SendCode("code", "service", null));
        }

        private void InitializeFakeObjects()
        {
            _twoFactorAuthenticationHandler = new TwoFactorAuthenticationHandler(null);
        }
    }
}
