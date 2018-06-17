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
using SimpleIdentityServer.Core.Common.Models;
using SimpleIdentityServer.Core.Common.Repositories;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Services;
using SimpleIdentityServer.Core.WebSite.User.Actions;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.WebSite.User
{
    public class AddUserOperationFixture
    {
        private Mock<IResourceOwnerRepository> _resourceOwnerRepositoryStub;
        private Mock<IAuthenticateResourceOwnerService> _authenticateResourceOwnerServiceStub;
        private Mock<IClaimRepository> _claimsRepositoryStub;
        private IAddUserOperation _addResourceOwnerAction;
        
        [Fact]
        public async Task When_Passing_Null_Parameters_Then_Exceptions_Are_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACTS & ASSERTS
            await Assert.ThrowsAsync<ArgumentNullException>(() => _addResourceOwnerAction.Execute(null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => _addResourceOwnerAction.Execute(new AddUserParameter(null, null)));
            await Assert.ThrowsAsync<ArgumentNullException>(() => _addResourceOwnerAction.Execute(new AddUserParameter("name", null)));
        }

        [Fact]
        public async Task When_ResourceOwner_With_Same_Credentials_Exists_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var parameter = new AddUserParameter("name", "password");

            _authenticateResourceOwnerServiceStub.Setup(r => r.AuthenticateResourceOwnerAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult(new ResourceOwner()));

            // ACT
            var exception = await Assert.ThrowsAsync<IdentityServerException>(() => _addResourceOwnerAction.Execute(parameter));

            // ASSERTS
            Assert.NotNull(exception);
            Assert.True(exception.Code == Errors.ErrorCodes.UnhandledExceptionCode);
            Assert.True(exception.Message == Errors.ErrorDescriptions.TheRoWithCredentialsAlreadyExists);
        }
        
        [Fact]
        public async Task When_Add_ResourceOwner_Then_Operation_Is_Called()
        {
            // ARRANGE
            InitializeFakeObjects();
            var parameter = new AddUserParameter("name", "password", new List<Claim>());

            _authenticateResourceOwnerServiceStub.Setup(r => r.AuthenticateResourceOwnerAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult((ResourceOwner)null));

            // ACT
            await _addResourceOwnerAction.Execute(parameter);

            // ASSERT
            _resourceOwnerRepositoryStub.Verify(r => r.InsertAsync(It.IsAny<ResourceOwner>()));
        }

        private void InitializeFakeObjects()
        {
            _resourceOwnerRepositoryStub = new Mock<IResourceOwnerRepository>();
            _claimsRepositoryStub = new Mock<IClaimRepository>();
            _authenticateResourceOwnerServiceStub = new Mock<IAuthenticateResourceOwnerService>();
            _addResourceOwnerAction = new AddUserOperation(
                _resourceOwnerRepositoryStub.Object,
                _claimsRepositoryStub.Object,
                _authenticateResourceOwnerServiceStub.Object);
        }
    }
}
