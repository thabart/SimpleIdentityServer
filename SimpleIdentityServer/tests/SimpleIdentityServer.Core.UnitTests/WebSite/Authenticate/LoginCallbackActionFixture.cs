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
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Core.WebSite.Authenticate.Actions;
using System;
using System.Security.Claims;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.WebSite.Authenticate
{
    public class LoginCallbackActionFixture
    {
        private Mock<IResourceOwnerRepository> _resourceOwnerRepositoryStub;

        private Mock<ISecurityHelper> _securityHelperStub;

        private ILoginCallbackAction _loginCallbackAction;

        #region Exceptions

        [Fact]
        public void When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ASSERT
            Assert.Throws<ArgumentNullException>(() => _loginCallbackAction.Execute(null));
        }

        [Fact]
        public void When_User_Is_Not_Authenticated_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var emptyClaimsPrincipal = new ClaimsPrincipal();

            // ACT
            var exception = Assert.Throws<IdentityServerException>(() => _loginCallbackAction.Execute(emptyClaimsPrincipal));

            // ASSERT
            Assert.NotNull(exception);
            Assert.True(exception.Code == Errors.ErrorCodes.UnhandledExceptionCode);
            Assert.True(exception.Message == Errors.ErrorDescriptions.TheUserNeedsToBeAuthenticated);
        }

        [Fact]
        public void When_Subject_Is_Not_Passed_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var claimsIdentity = new ClaimsIdentity("test");
            claimsIdentity.AddClaim(new Claim("test", "test"));
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            // ACT
            var exception = Assert.Throws<IdentityServerException>(() => _loginCallbackAction.Execute(claimsPrincipal));

            // ASSERT
            Assert.NotNull(exception);
            Assert.True(exception.Code == Errors.ErrorCodes.UnhandledExceptionCode);
            Assert.True(exception.Message == Errors.ErrorDescriptions.TheRoCannotBeCreated);
        }

        #endregion

        #region Happy path

        [Fact]
        public void When_Ro_Exists_Then_No_Ro_Is_Inserted()
        {
            // ARRANGE
            InitializeFakeObjects();
            var claimsIdentity = new ClaimsIdentity("test");
            claimsIdentity.AddClaim(new Claim(Jwt.Constants.StandardResourceOwnerClaimNames.Subject, "subject"));
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            _resourceOwnerRepositoryStub.Setup(r => r.GetBySubject(It.IsAny<string>()))
                .Returns(new ResourceOwner());

            // ACT
            _loginCallbackAction.Execute(claimsPrincipal);

            // ASSERT
            _resourceOwnerRepositoryStub.Verify(r => r.Insert(It.IsAny<ResourceOwner>()), Times.Never);
        }

        [Fact]
        public void When_Claims_Are_Passed_Then_New_ResourceOwner_Is_Inserted()
        {
            // ARRANGE
            InitializeFakeObjects();
            var claimsIdentity = new ClaimsIdentity("test");
            claimsIdentity.AddClaim(new Claim(Jwt.Constants.StandardResourceOwnerClaimNames.Subject, "subject"));
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            _resourceOwnerRepositoryStub.Setup(r => r.GetBySubject(It.IsAny<string>()))
                .Returns((ResourceOwner)null);

            // ACT
            _loginCallbackAction.Execute(claimsPrincipal);

            // ASSERT
            _resourceOwnerRepositoryStub.Verify(r => r.Insert(It.IsAny<ResourceOwner>()));
        }

        #endregion

        #region Private methods

        private void InitializeFakeObjects()
        {
            _resourceOwnerRepositoryStub = new Mock<IResourceOwnerRepository>();
            _securityHelperStub = new Mock<ISecurityHelper>();
            _loginCallbackAction = new LoginCallbackAction(
                _resourceOwnerRepositoryStub.Object,
                _securityHelperStub.Object);
        }

        #endregion
    }
}
