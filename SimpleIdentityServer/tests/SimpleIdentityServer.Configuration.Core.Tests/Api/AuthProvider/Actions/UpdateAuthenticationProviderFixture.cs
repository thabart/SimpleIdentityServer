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
    public class UpdateAuthenticationProviderFixture
    {
        private Mock<IAuthenticationProviderRepository> _authenticationProviderRepositoryStub;
        private IUpdateAuthenticationProvider _updateAuthenticationProvider;

        [Fact]
        public void When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            Assert.ThrowsAsync<ArgumentNullException>(() => _updateAuthenticationProvider.ExecuteAsync(null));
        }
        
        [Fact]
        public async Task When_AuthenticationProvider_Doesnt_Exist_Then_Not_Found_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _authenticationProviderRepositoryStub.Setup(a => a.GetAuthenticationProvider(It.IsAny<string>()))
                .Returns(Task.FromResult<AuthenticationProvider>(null));

            // ACT
            var result = await _updateAuthenticationProvider.ExecuteAsync(new AuthenticationProvider
            {
                Name = "name"
            });

            // ASSERTS
            Assert.NotNull(result);
            Assert.True(result is NotFoundResult);
        }

        [Fact]
        public async Task When_AuthenticationProvider_Has_Been_Updated_Then_NoContent_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _authenticationProviderRepositoryStub.Setup(a => a.GetAuthenticationProvider(It.IsAny<string>()))
                .Returns(Task.FromResult(new AuthenticationProvider
                {
                    Name = "name"
                }));
            _authenticationProviderRepositoryStub.Setup(a => a.UpdateAuthenticationProvider(It.IsAny<AuthenticationProvider>()))
                .Returns(Task.FromResult(true));

            // ACT
            var result = await _updateAuthenticationProvider.ExecuteAsync(new AuthenticationProvider
            {
                Name = "name"
            });

            // ASSERTS
            Assert.NotNull(result);
            Assert.True(result is NoContentResult);
        }

        [Fact]
        public async Task When_AuthenticationProvider_Cannot_Be_Updated_Then_InernalError_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _authenticationProviderRepositoryStub.Setup(a => a.GetAuthenticationProvider(It.IsAny<string>()))
                .Returns(Task.FromResult(new AuthenticationProvider
                {
                    Name = "name"
                }));
            _authenticationProviderRepositoryStub.Setup(a => a.UpdateAuthenticationProvider(It.IsAny<AuthenticationProvider>()))
                .Returns(Task.FromResult(false));

            // ACT
            var result = await _updateAuthenticationProvider.ExecuteAsync(new AuthenticationProvider
            {
                Name = "name"
            });

            // ASSERTS
            Assert.NotNull(result);
            Assert.True(result is StatusCodeResult);
            Assert.True((result as StatusCodeResult).StatusCode == StatusCodes.Status500InternalServerError);
        }

        private void InitializeFakeObjects()
        {
            _authenticationProviderRepositoryStub = new Mock<IAuthenticationProviderRepository>();
            _updateAuthenticationProvider = new UpdateAuthenticationProvider(_authenticationProviderRepositoryStub.Object);
        }
    }
}
