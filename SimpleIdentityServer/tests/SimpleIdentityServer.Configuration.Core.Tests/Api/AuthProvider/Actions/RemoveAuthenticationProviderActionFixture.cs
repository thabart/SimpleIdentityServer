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

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SimpleIdentityServer.Configuration.Core.Api.AuthProvider.Actions;
using SimpleIdentityServer.Configuration.Core.Models;
using SimpleIdentityServer.Configuration.Core.Repositories;
using SimpleIdentityServer.Logging;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Configuration.Core.Tests.Api.AuthProvider.Actions
{
    public class RemoveAuthenticationProviderActionFixture
    {
        private Mock<IAuthenticationProviderRepository> _authenticationProviderRepositoryStub;
        private Mock<IConfigurationEventSource> _configurationEventSourceStub;
        private IRemoveAuthenticationProviderAction _removeAuthenticationProviderAction;

        [Fact]
        public void When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            Assert.ThrowsAsync<ArgumentNullException>(() => _removeAuthenticationProviderAction.ExecuteAsync(null));
        }

        [Fact]
        public async Task When_Removing_Unexisting_AuthenticationProvider_Then_NotFound_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _authenticationProviderRepositoryStub.Setup(a => a.GetAuthenticationProvider(It.IsAny<string>()))
                .Returns(Task.FromResult<AuthenticationProvider>(null));

            // ACT
            var result = await _removeAuthenticationProviderAction.ExecuteAsync("name").ConfigureAwait(false);

            // ASSERT
            Assert.NotNull(result);
            Assert.True(result is NotFoundResult);
        }

        [Fact]
        public async Task When_AuthenticationProvider_Cannot_Be_Removed_Then_HttpStatusCode_500_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _authenticationProviderRepositoryStub.Setup(a => a.GetAuthenticationProvider(It.IsAny<string>()))
                .Returns(Task.FromResult(new AuthenticationProvider()));
            _authenticationProviderRepositoryStub.Setup(a => a.RemoveAuthenticationProvider(It.IsAny<string>()))
                .Returns(Task.FromResult(false));

            // ACT
            var result = await _removeAuthenticationProviderAction.ExecuteAsync("name").ConfigureAwait(false) as StatusCodeResult;

            // ASSERT
            Assert.NotNull(result);
            Assert.True(result.StatusCode == StatusCodes.Status500InternalServerError);
        }

        [Fact]
        public async Task When_AuthenticationProvider_Is_Removed_Then_NoContent_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _authenticationProviderRepositoryStub.Setup(a => a.GetAuthenticationProvider(It.IsAny<string>()))
                .Returns(Task.FromResult(new AuthenticationProvider()));
            _authenticationProviderRepositoryStub.Setup(a => a.RemoveAuthenticationProvider(It.IsAny<string>()))
                .Returns(Task.FromResult(true));

            // ACT
            var result = await _removeAuthenticationProviderAction.ExecuteAsync("name").ConfigureAwait(false) as StatusCodeResult;

            // ASSERT
            Assert.NotNull(result);
            Assert.True(result.StatusCode == StatusCodes.Status204NoContent);
        }
        
        private void InitializeFakeObjects()
        {
            _authenticationProviderRepositoryStub = new Mock<IAuthenticationProviderRepository>();
            _configurationEventSourceStub = new Mock<IConfigurationEventSource>();
            _removeAuthenticationProviderAction = new RemoveAuthenticationProviderAction(
                _authenticationProviderRepositoryStub.Object,
                _configurationEventSourceStub.Object);
        }
    }
}
