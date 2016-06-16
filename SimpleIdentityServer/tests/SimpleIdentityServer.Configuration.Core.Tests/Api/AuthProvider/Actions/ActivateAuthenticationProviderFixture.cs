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
using System;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Configuration.Core.Tests.Api.AuthProvider.Actions
{
    public class ActivateAuthenticationProviderFixture
    {
        private Mock<IAuthenticationProviderRepository> _authenticationProviderRepositoryStub;

        private IActivateAuthenticationProvider _activateAuthenticationProvider;

        #region Exceptions

        [Fact]
        public void When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            Assert.ThrowsAsync<ArgumentNullException>(() => _activateAuthenticationProvider.ExecuteAsync(null, false));
        }

        #endregion

        #region Happy Paths

        [Fact]
        public async Task When_AuthenticationProvider_Doesnt_Exist_Then_NotFound_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _authenticationProviderRepositoryStub.Setup(a => a.GetAuthenticationProvider(It.IsAny<string>()))
                .Returns(Task.FromResult<AuthenticationProvider>(null));

            // ACT
            var result = await _activateAuthenticationProvider.ExecuteAsync("name", false);

            // ASSERT
            Assert.NotNull(result);
            Assert.True(result is NotFoundResult);
        }

        [Fact]
        public async Task When_AuthenticationProvider_Is_Updated_Then_NoContent_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _authenticationProviderRepositoryStub.Setup(a => a.GetAuthenticationProvider(It.IsAny<string>()))
                .Returns(Task.FromResult(new AuthenticationProvider()));
            _authenticationProviderRepositoryStub.Setup(a => a.UpdateAuthenticationProvider(It.IsAny<AuthenticationProvider>()))
                .Returns(Task.FromResult(true));

            // ACT
            var result = await _activateAuthenticationProvider.ExecuteAsync("name", false);

            // ASSERT
            Assert.NotNull(result);
            Assert.True(result is NoContentResult);
        }


        [Fact]
        public async Task When_AuthenticationProvider_Is_Not_Updated_Then_InternalError_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _authenticationProviderRepositoryStub.Setup(a => a.GetAuthenticationProvider(It.IsAny<string>()))
                .Returns(Task.FromResult(new AuthenticationProvider()));
            _authenticationProviderRepositoryStub.Setup(a => a.UpdateAuthenticationProvider(It.IsAny<AuthenticationProvider>()))
                .Returns(Task.FromResult(false));

            // ACT
            var result = await _activateAuthenticationProvider.ExecuteAsync("name", false);

            // ASSERT
            Assert.NotNull(result);
            Assert.True(result is StatusCodeResult);
            Assert.True((result as StatusCodeResult).StatusCode == StatusCodes.Status500InternalServerError);
        }

        #endregion

        #region Private methods

        private void InitializeFakeObjects()
        {
            _authenticationProviderRepositoryStub = new Mock<IAuthenticationProviderRepository>();
            _activateAuthenticationProvider = new ActivateAuthenticationProvider(_authenticationProviderRepositoryStub.Object);
        }

        #endregion
    }
}
