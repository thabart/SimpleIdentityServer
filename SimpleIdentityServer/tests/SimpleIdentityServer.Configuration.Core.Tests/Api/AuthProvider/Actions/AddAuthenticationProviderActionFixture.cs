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
using SimpleIdentityServer.Configuration.Core.Errors;
using SimpleIdentityServer.Configuration.Core.Exceptions;
using SimpleIdentityServer.Configuration.Core.Models;
using SimpleIdentityServer.Configuration.Core.Repositories;
using SimpleIdentityServer.Logging;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Configuration.Core.Tests.Api.AuthProvider.Actions
{
    public class AddAuthenticationProviderActionFixture
    {
        private Mock<IAuthenticationProviderRepository> _authenticationProviderRepository;
        private Mock<IConfigurationEventSource> _configurationEventSourceStub;
        private IAddAuthenticationProviderAction _addAuthenticationProviderAction;

        [Fact]
        public void When_Passing_Null_Parameters_Then_Exceptions_Are_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACTS & ASSERTS
            Assert.ThrowsAsync<ArgumentNullException>(() => _addAuthenticationProviderAction.ExecuteAsync(null));
            Assert.ThrowsAsync<ArgumentNullException>(() => _addAuthenticationProviderAction.ExecuteAsync(new AuthenticationProvider()));
            Assert.ThrowsAsync<ArgumentNullException>(() => _addAuthenticationProviderAction.ExecuteAsync(new AuthenticationProvider
            {
                Name = "name"
            }));
            Assert.ThrowsAsync<ArgumentNullException>(() => _addAuthenticationProviderAction.ExecuteAsync(null));
        }

        [Fact]
        public async Task When_AuthenticationProvider_Already_Exists_Then_Exception_Is_Thrown()
        {
            const string name = "name";
            // ARRANGE
            InitializeFakeObjects();
            _authenticationProviderRepository.Setup(a => a.GetAuthenticationProvider(It.IsAny<string>()))
                .Returns(Task.FromResult(new AuthenticationProvider
                {
                    Name = name
                }));

            // ACT & ASSERTS
            var exception = await Assert.ThrowsAsync<IdentityConfigurationException>(() => _addAuthenticationProviderAction.ExecuteAsync(new AuthenticationProvider
            {
                Name = name,
                CallbackPath = "callback_path"
            })).ConfigureAwait(false);
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InternalErrorCode);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheAuthenticationProviderAlreadyExists, name));
        }

        [Fact]
        public async Task When_AuthenticationProvider_Cannot_Be_Inserted_Then_HttpStatusCode500_Is_Returned()
        {
            const string name = "name";
            // ARRANGE
            InitializeFakeObjects();
            _authenticationProviderRepository.Setup(a => a.GetAuthenticationProvider(It.IsAny<string>()))
                .Returns(Task.FromResult<AuthenticationProvider>(null));
            _authenticationProviderRepository.Setup(a => a.AddAuthenticationProvider(It.IsAny<AuthenticationProvider>()))
                .Returns(Task.FromResult(false));

            // ACT
            var result = await _addAuthenticationProviderAction.ExecuteAsync(new AuthenticationProvider
            {
                Name = name,
                CallbackPath = "callback_path"
            }).ConfigureAwait(false) as StatusCodeResult;

            // ASSERTS
            Assert.NotNull(result);
            Assert.True(result.StatusCode == StatusCodes.Status500InternalServerError);
        }

        [Fact]
        public async Task When_AuthenticationProvider_Is_Inserted_Then_NoContent_Is_Returned()
        {
            const string name = "name";
            // ARRANGE
            InitializeFakeObjects();
            _authenticationProviderRepository.Setup(a => a.GetAuthenticationProvider(It.IsAny<string>()))
                .Returns(Task.FromResult<AuthenticationProvider>(null));
            _authenticationProviderRepository.Setup(a => a.AddAuthenticationProvider(It.IsAny<AuthenticationProvider>()))
                .Returns(Task.FromResult(true));

            // ACT
            var result = await _addAuthenticationProviderAction.ExecuteAsync(new AuthenticationProvider
            {
                Name = name,
                CallbackPath = "callback_path"
            }).ConfigureAwait(false) as StatusCodeResult;

            // ASSERTS
            Assert.NotNull(result);
            Assert.True(result.StatusCode == StatusCodes.Status204NoContent);
        }

        private void InitializeFakeObjects()
        {
            _authenticationProviderRepository = new Mock<IAuthenticationProviderRepository>();
            _configurationEventSourceStub = new Mock<IConfigurationEventSource>();
            _addAuthenticationProviderAction = new AddAuthenticationProviderAction(
                _authenticationProviderRepository.Object,
                _configurationEventSourceStub.Object);
        }
    }
}
