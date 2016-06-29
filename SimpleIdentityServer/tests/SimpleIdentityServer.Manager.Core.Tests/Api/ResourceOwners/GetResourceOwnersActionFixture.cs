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
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Manager.Core.Api.ResourceOwners.Actions;
using Xunit;

namespace SimpleIdentityServer.Manager.Core.Tests.Api.ResourceOwners
{
    public class GetResourceOwnersActionFixture
    {
        private Mock<IResourceOwnerRepository> _resourceOwnerRepositoryStub;

        private IGetResourceOwnersAction _getResourceOwnersAction;

        #region Happy path

        [Fact]
        public void When_Getting_ResourceOwners_Then_Operation_Is_Called()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT
            _getResourceOwnersAction.Execute();

            // ASSERT
            _resourceOwnerRepositoryStub.Verify(r => r.GetAll());
        }

        #endregion

        #region Private methods

        private void InitializeFakeObjects()
        {
            _resourceOwnerRepositoryStub = new Mock<IResourceOwnerRepository>();
            _getResourceOwnersAction = new GetResourceOwnersAction(_resourceOwnerRepositoryStub.Object);
        }

        #endregion
    }
}
