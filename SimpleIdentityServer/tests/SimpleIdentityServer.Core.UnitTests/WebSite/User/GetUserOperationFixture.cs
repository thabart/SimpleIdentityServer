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
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Core.Services;
using SimpleIdentityServer.Core.WebSite.User.Actions;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.WebSite.User
{
    public class GetUserOperationFixture
    {
        private Mock<IResourceOwnerRepository> _resourceOwnerRepositoryStub;
        private Mock<IAuthenticateResourceOwnerService> _authenticateResourceOwnerServiceStub;
        private IGetUserOperation _getUserOperation;

        [Fact]
        public async Task When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ASSERT
            await Assert.ThrowsAsync<ArgumentNullException>(() => _getUserOperation.Execute(null));
        }

        [Fact]
        public async Task When_User_Is_Not_Authenticated_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var emptyClaimsPrincipal = new ClaimsPrincipal();

            // ACT
            var exception = await Assert.ThrowsAsync<IdentityServerException>(() => _getUserOperation.Execute(emptyClaimsPrincipal));

            // ASSERT
            Assert.NotNull(exception);
            Assert.True(exception.Code == Errors.ErrorCodes.UnhandledExceptionCode);
            Assert.True(exception.Message == Errors.ErrorDescriptions.TheUserNeedsToBeAuthenticated);
        }

        [Fact]
        public async Task When_Subject_Is_Not_Passed_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var claimsIdentity = new ClaimsIdentity("test");
            claimsIdentity.AddClaim(new Claim("test", "test"));
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            // ACT
            var exception = await  Assert.ThrowsAsync<IdentityServerException>(() => _getUserOperation.Execute(claimsPrincipal));

            // ASSERT
            Assert.NotNull(exception);
            Assert.True(exception.Code == Errors.ErrorCodes.UnhandledExceptionCode);
            Assert.True(exception.Message == Errors.ErrorDescriptions.TheSubjectCannotBeRetrieved);
        }
        
        [Fact]
        public async Task When_Ro_DoesntExist_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var claimsIdentity = new ClaimsIdentity("test");
            claimsIdentity.AddClaim(new Claim(Jwt.Constants.StandardResourceOwnerClaimNames.Subject, "subject"));
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            _authenticateResourceOwnerServiceStub.Setup(r => r.AuthenticateResourceOwnerAsync(It.IsAny<string>()))
                .Returns(Task.FromResult((ResourceOwner)null));

            // ACT
            var exception = await Assert.ThrowsAsync<IdentityServerException>(() => _getUserOperation.Execute(claimsPrincipal));

            // ASSERT
            Assert.NotNull(exception);
            Assert.True(exception.Code == Errors.ErrorCodes.UnhandledExceptionCode);
            Assert.True(exception.Message == Errors.ErrorDescriptions.TheRoDoesntExist);
        }
        
        [Fact]
        public async Task When_Correct_Subject_Is_Passed_Then_ResourceOwner_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            var claimsIdentity = new ClaimsIdentity("test");
            claimsIdentity.AddClaim(new Claim(Jwt.Constants.StandardResourceOwnerClaimNames.Subject, "subject"));
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            _authenticateResourceOwnerServiceStub.Setup(r => r.AuthenticateResourceOwnerAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(new ResourceOwner()));

            // ACT
            var result = _getUserOperation.Execute(claimsPrincipal);

            // ASSERT
            Assert.NotNull(result);
        }
        
        private void InitializeFakeObjects()
        {
            _resourceOwnerRepositoryStub = new Mock<IResourceOwnerRepository>();
            _authenticateResourceOwnerServiceStub = new Mock<IAuthenticateResourceOwnerService>();
            _getUserOperation = new GetUserOperation(
                _resourceOwnerRepositoryStub.Object,
                _authenticateResourceOwnerServiceStub.Object);
        }
    }
}
