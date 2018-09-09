﻿#region copyright
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

using System.Linq;
using Moq;
using SimpleIdentityServer.Uma.Core.Api.ResourceSetController.Actions;
using SimpleIdentityServer.Uma.Core.Errors;
using SimpleIdentityServer.Uma.Core.Exceptions;
using SimpleIdentityServer.Uma.Core.Models;
using SimpleIdentityServer.Uma.Core.Repositories;
using System.Collections.Generic;
using Xunit;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Uma.Core.UnitTests.Api.ResourceSetController.Actions
{
    public class GetAllResourceSetActionFixture
    {
        private Mock<IResourceSetRepository> _resourceSetRepositoryStub;
        private IGetAllResourceSetAction _getAllResourceSetAction;

        [Fact]
        public async Task When_Error_Occured_While_Trying_To_Retrieve_ResourceSet_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObject();
            _resourceSetRepositoryStub.Setup(r => r.GetAll())
                .Returns(() => Task.FromResult((ICollection<ResourceSet>)null));

            // ACT & ASSERTS
            var exception  = await Assert.ThrowsAsync<BaseUmaException>(() => _getAllResourceSetAction.Execute()).ConfigureAwait(false);
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InternalError);
            Assert.True(exception.Message == ErrorDescriptions.TheResourceSetsCannotBeRetrieved);
        }

        [Fact]
        public async Task When_ResourceSets_Are_Retrieved_Then_Ids_Are_Returned()
        {
            // ARRANGE
            const string id = "id";
            ICollection<ResourceSet> resourceSets = new List<ResourceSet>
            {
                new ResourceSet
                {
                    Id = id
                }
            };
            InitializeFakeObject();
            _resourceSetRepositoryStub.Setup(r => r.GetAll())
                .Returns(Task.FromResult(resourceSets));

            // ACT
            var result = await _getAllResourceSetAction.Execute().ConfigureAwait(false);

            // ASSERTS
            Assert.NotNull(result);
            Assert.True(result.Count() == 1);
            Assert.True(result.First() == id);
        }

        private void InitializeFakeObject()
        {
            _resourceSetRepositoryStub = new Mock<IResourceSetRepository>();
            _getAllResourceSetAction = new GetAllResourceSetAction(_resourceSetRepositoryStub.Object);
        }
    }
}
