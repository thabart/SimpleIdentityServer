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

using Moq;
using SimpleIdentityServer.Core.Bus;
using SimpleIdentityServer.Core.Events;
using System.Linq;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.Bus
{
    public class EvtHandlerStoreFixture
    {
        private IEvtHandlerStore _evtHandlerStore;

        [Fact]
        public void When_Handler_Is_Saved_And_Retrieve_It_Then_Handler_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            var firstHandler = (new Mock<IHandle<AuthorizationRequestReceived>>()).Object;
            var secondHandler = (new Mock<IHandle<AuthorizationRequestReceived>>()).Object;
            _evtHandlerStore.Register(firstHandler);
            _evtHandlerStore.Register(secondHandler);

            // ACT
            var handlers = _evtHandlerStore.Get<AuthorizationRequestReceived>();

            // ASSERTS
            Assert.NotNull(handlers);
            Assert.True(handlers.Count() == 2);
        }

        private void InitializeFakeObjects()
        {
            _evtHandlerStore = new EvtHandlerStore();
        }
    }
}
