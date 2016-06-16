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
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Configuration.Core.Tests.Api.AuthProvider.Actions
{
    public class GetAuthenticationProvidersFixture
    {
        private Mock<IAuthenticationProviderRepository> _authenticationProviderRepositoryStub;

        private IGetAuthenticationProviders _getAuthenticationProviders;

        #region Happy path

        [Fact]
        public async Task When_Error_Occured_While_Trying_To_Retrieve_The_AuthorizationProviders_Then_InternalError_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _authenticationProviderRepositoryStub.Setup(a => a.GetAuthenticationProviders())
                .Returns(Task.FromResult<List<AuthenticationProvider>>(null));

            // ACT
            var result = await _getAuthenticationProviders.ExecuteAsync();

            // ARRANGE
            Assert.NotNull(result);
            Assert.NotNull(result as StatusCodeResult);
            Assert.True((result as StatusCodeResult).StatusCode == StatusCodes.Status500InternalServerError);
        }

        [Fact]
        public async Task When_AuthorizationProviders_Are_Retrieved_Then_Ok_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _authenticationProviderRepositoryStub.Setup(a => a.GetAuthenticationProviders())
                .Returns(Task.FromResult(new List<AuthenticationProvider>()));

            // ACT
            var result = await _getAuthenticationProviders.ExecuteAsync();

            // ARRANGE
            Assert.NotNull(result);
            Assert.True(result is OkObjectResult);
        }


        #endregion

        #region Private methods

        private void InitializeFakeObjects()
        {
            _authenticationProviderRepositoryStub = new Mock<IAuthenticationProviderRepository>();
            _getAuthenticationProviders = new GetAuthenticationProviders(_authenticationProviderRepositoryStub.Object);
        }

        #endregion
    }
}
