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
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Manager.Core.Api.ResourceOwners.Actions;
using SimpleIdentityServer.Manager.Core.Errors;
using SimpleIdentityServer.Manager.Core.Exceptions;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Manager.Core.Tests.Api.ResourceOwners
{
    public class UpdateResourceOwnerActionFixture
    {
        private Mock<IResourceOwnerRepository> _resourceOwnerRepositoryStub;
        private IUpdateResourceOwnerAction _updateResourceOwnerAction;

        [Fact]
        public async Task When_Passing_Null_Parameters_Then_Exceptions_Are_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            await Assert.ThrowsAsync<ArgumentNullException>(() => _updateResourceOwnerAction.Execute(null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => _updateResourceOwnerAction.Execute(new ResourceOwner()));
        }

        [Fact]
        public async Task When_ResourceOwner_Doesnt_Exist_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            const string subject = "invalid_subject";
            var request = new ResourceOwner
            {
                Id = subject
            };
            InitializeFakeObjects();
            _resourceOwnerRepositoryStub.Setup(r => r.GetAsync(It.IsAny<string>()))
                .Returns(Task.FromResult((ResourceOwner)null));

            // ACT
            var exception = await Assert.ThrowsAsync<IdentityServerManagerException>(() => _updateResourceOwnerAction.Execute(request));

            // ASSERT
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidParameterCode);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheResourceOwnerDoesntExist, subject));
        }
        
        [Fact]
        public async Task When_Updating_Resource_Owner_Then_Operation_Is_Called()
        {
            // ARRANGE
            var request = new ResourceOwner
            {
                Id = "subject"
            };
            InitializeFakeObjects();
            _resourceOwnerRepositoryStub.Setup(r => r.GetAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(new ResourceOwner
                {
                    IsLocalAccount = true
                }));

            // ACT
            await _updateResourceOwnerAction.Execute(request);

            // ASSERT
            _resourceOwnerRepositoryStub.Verify(r => r.UpdateAsync(It.IsAny<ResourceOwner>()));
        }

        private void InitializeFakeObjects()
        {
            _resourceOwnerRepositoryStub = new Mock<IResourceOwnerRepository>();
            _updateResourceOwnerAction = new UpdateResourceOwnerAction(
                _resourceOwnerRepositoryStub.Object);
        }
    }
}
