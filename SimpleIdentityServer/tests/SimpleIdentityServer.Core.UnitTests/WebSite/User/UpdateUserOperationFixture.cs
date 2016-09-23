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
using SimpleIdentityServer.Core.WebSite.User.Actions;
using System;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.WebSite.User
{
    public class UpdateUserOperationFixture
    {
        private Mock<IResourceOwnerRepository> _resourceOwnerRepositoryStub;

        private Mock<ISecurityHelper> _securityHelperStub;

        private IUpdateUserOperation _updateUserOperation;

        #region Exceptions

        [Fact]
        public void When_Passing_Null_Parameters_Then_Exceptions_Are_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACTS & ASSERTS
            Assert.Throws<ArgumentNullException>(() => _updateUserOperation.Execute(null));
            Assert.Throws<ArgumentNullException>(() => _updateUserOperation.Execute(new UpdateUserParameter()));
            Assert.Throws<ArgumentNullException>(() => _updateUserOperation.Execute(new UpdateUserParameter
            {
                Id = "id"
            }));
        }

        [Fact]
        public void When_EmailAddress_Is_Not_Valid_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            Assert.Throws<ArgumentException>(() => _updateUserOperation.Execute(new UpdateUserParameter
            {
                Id = "id",
                Name = "name",
                Password = "password",
                Email = "invalid_email"
            }));
        }

        [Fact]
        public void When_ResourceOwner_DoesntExist_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var parameter = new UpdateUserParameter
            {
                Id = "id",
                Name = "name",
                Password = "password"
            };
            _resourceOwnerRepositoryStub.Setup(r => r.GetBySubject(It.IsAny<string>()))
                .Returns((ResourceOwner)null);

            // ACT
            var exception = Assert.Throws<IdentityServerException>(() => _updateUserOperation.Execute(parameter));

            // ASSERTS
            Assert.NotNull(exception);
            Assert.True(exception.Code == Errors.ErrorCodes.InternalError);
            Assert.True(exception.Message == Errors.ErrorDescriptions.TheRoDoesntExist);
        }

        [Fact]
        public void When_ResourceOwner_With_SameCredentials_Exists_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var parameter = new UpdateUserParameter
            {
                Id = "id",
                Name = "name",
                Password = "password"
            };
            _resourceOwnerRepositoryStub.Setup(r => r.GetBySubject(It.IsAny<string>()))
                .Returns(new ResourceOwner());
            _resourceOwnerRepositoryStub.Setup(r => r.GetResourceOwnerByCredentials(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new ResourceOwner
                {
                    Id = "id2"
                });

            // ACT
            var exception = Assert.Throws<IdentityServerException>(() => _updateUserOperation.Execute(parameter));

            // ASSERTS
            Assert.NotNull(exception);
            Assert.True(exception.Code == Errors.ErrorCodes.InternalError);
            Assert.True(exception.Message == Errors.ErrorDescriptions.TheRoWithCredentialsAlreadyExists);
        }
                
        #endregion

        #region Happy path

        [Fact]
        public void When_Passing_Correct_Parameters_Then_ResourceOwnerIs_Updated()
        {
            // ARRANGE
            InitializeFakeObjects();
            var parameter = new UpdateUserParameter
            {
                Id = "id",
                Name = "name",
                Password = "password"
            };
            _resourceOwnerRepositoryStub.Setup(r => r.GetResourceOwnerByCredentials(It.IsAny<string>(), It.IsAny<string>()))
                .Returns((ResourceOwner)null);
            _resourceOwnerRepositoryStub.Setup(r => r.GetBySubject(It.IsAny<string>()))
                .Returns(new ResourceOwner());

            // ACT
            _updateUserOperation.Execute(parameter);

            // ASSERTS
            _resourceOwnerRepositoryStub.Setup(r => r.Update(It.IsAny<ResourceOwner>()));
        }

        #endregion

        #region Private methods

        private void InitializeFakeObjects()
        {
            _resourceOwnerRepositoryStub = new Mock<IResourceOwnerRepository>();
            _securityHelperStub = new Mock<ISecurityHelper>();
            _updateUserOperation = new UpdateUserOperation(
                _resourceOwnerRepositoryStub.Object,
                _securityHelperStub.Object);
        }

        #endregion
    }
}
