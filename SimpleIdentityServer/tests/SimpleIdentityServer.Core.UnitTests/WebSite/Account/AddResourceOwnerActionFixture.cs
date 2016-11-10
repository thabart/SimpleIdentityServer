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
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Core.Services;
using SimpleIdentityServer.Core.WebSite.Account.Actions;
using System;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.WebSite.Account
{
    public class AddResourceOwnerActionFixture
    {
        private Mock<IResourceOwnerRepository> _resourceOwnerRepositoryStub;
        private Mock<ISecurityHelper> _securityHelperStub;
        private Mock<IAuthenticateResourceOwnerService> _authenticateResourceOwnerServiceStub;
        private IAddResourceOwnerAction _addResourceOwnerAction;
        
        [Fact]
        public void When_Passing_Null_Parameters_Then_Exceptions_Are_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACTS & ASSERTS
            Assert.Throws<ArgumentNullException>(() => _addResourceOwnerAction.Execute(null));
            Assert.Throws<ArgumentNullException>(() => _addResourceOwnerAction.Execute(new AddUserParameter()));
            Assert.Throws<ArgumentNullException>(() => _addResourceOwnerAction.Execute(new AddUserParameter
            {
                Login = "name"
            }));
        }

        [Fact]
        public void When_ResourceOwner_With_Same_Credentials_Exists_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var parameter = new AddUserParameter
            {
                Login = "name",
                Password = "password"
            };

            _authenticateResourceOwnerServiceStub.Setup(r => r.AuthenticateResourceOwner(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new ResourceOwner());

            // ACT
            var exception = Assert.Throws<IdentityServerException>(() => _addResourceOwnerAction.Execute(parameter));

            // ASSERTS
            Assert.NotNull(exception);
            Assert.True(exception.Code == Errors.ErrorCodes.UnhandledExceptionCode);
            Assert.True(exception.Message == Errors.ErrorDescriptions.TheRoWithCredentialsAlreadyExists);
        }
        
        [Fact]
        public void When_Add_ResourceOwner_Then_Operation_Is_Called()
        {
            // ARRANGE
            InitializeFakeObjects();
            var parameter = new AddUserParameter
            {
                Login = "name",
                Password = "password"
            };

            _authenticateResourceOwnerServiceStub.Setup(r => r.AuthenticateResourceOwner(It.IsAny<string>(), It.IsAny<string>()))
                .Returns((ResourceOwner)null);

            // ACT
            _addResourceOwnerAction.Execute(parameter);

            // ASSERT
            _resourceOwnerRepositoryStub.Verify(r => r.Insert(It.IsAny<ResourceOwner>()));
        }

        private void InitializeFakeObjects()
        {
            _resourceOwnerRepositoryStub = new Mock<IResourceOwnerRepository>();
            _securityHelperStub = new Mock<ISecurityHelper>();
            _authenticateResourceOwnerServiceStub = new Mock<IAuthenticateResourceOwnerService>();
            _addResourceOwnerAction = new AddResourceOwnerAction(
                _resourceOwnerRepositoryStub.Object,
                _securityHelperStub.Object,
                _authenticateResourceOwnerServiceStub.Object);
        }
    }
}
