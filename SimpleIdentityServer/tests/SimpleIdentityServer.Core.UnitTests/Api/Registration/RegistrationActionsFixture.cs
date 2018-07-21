﻿#region copyright
// Copyright 2017 Habart Thierry
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

using System;
using Moq;
using SimpleIdentityServer.Core.Api.Registration;
using SimpleIdentityServer.Core.Api.Registration.Actions;
using SimpleIdentityServer.Core.Parameters;
using Xunit;
using System.Threading.Tasks;
using SimpleIdentityServer.Core.Bus;

namespace SimpleIdentityServer.Core.UnitTests.Api.Registration
{
    public sealed class RegistrationActionsFixture
    {
        private Mock<IRegisterClientAction> _registerClientActionFake;
        private IRegistrationActions _registrationActions;

        [Fact]
        public async Task When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            await Assert.ThrowsAsync<ArgumentNullException>(() => _registrationActions.PostRegistration(null)).ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Passing_Valid_Parameter_Then_Action_Is_Called()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT
            await _registrationActions.PostRegistration(new RegistrationParameter()).ConfigureAwait(false);

            // ASSERT
            _registerClientActionFake.Verify(r => r.Execute(It.IsAny<RegistrationParameter>()));
        }

        private void InitializeFakeObjects()
        {
            _registerClientActionFake = new Mock<IRegisterClientAction>();
            var eventPublisherStub = new Mock<IEventPublisher>();
            _registrationActions = new RegistrationActions(_registerClientActionFake.Object, eventPublisherStub.Object);
        }
    }
}
