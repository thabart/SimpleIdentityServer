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
using SimpleIdentityServer.Scim.Common.Models;
using SimpleIdentityServer.Scim.Core.Apis;
using SimpleIdentityServer.Scim.Core.Errors;
using SimpleIdentityServer.Scim.Core.Factories;
using SimpleIdentityServer.Scim.Core.Results;
using SimpleIdentityServer.Scim.Core.Stores;
using System;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Scim.Core.Tests.Apis
{
    public class DeleteRepresentationActionFixture
    {
        private Mock<IRepresentationStore> _representationStoreStub;
        private Mock<IApiResponseFactory> _apiResponseFactoryStub;
        private IDeleteRepresentationAction _deleteRepresentationAction;

        [Fact]
        public async Task When_Passing_Null_Parameters_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            await Assert.ThrowsAsync<ArgumentNullException>(() => _deleteRepresentationAction.Execute(null));
        }

        [Fact]
        public async Task When_Representation_Doesnt_Exist_Then_404_Code_Is_Returned()
        {
            // ARRANGE
            const string identifier = "identifier";
            InitializeFakeObjects();
            _representationStoreStub.Setup(r => r.GetRepresentation(It.IsAny<string>()))
                .Returns(Task.FromResult((Representation)null));
            _apiResponseFactoryStub.Setup(a => a.CreateError(HttpStatusCode.NotFound, string.Format(ErrorMessages.TheResourceDoesntExist, identifier)))
                .Returns(new ApiActionResult
                {
                    StatusCode = (int)HttpStatusCode.NotFound
                });

            // ACT
            var result = await _deleteRepresentationAction.Execute(identifier).ConfigureAwait(false);

            // ASSERT
            Assert.NotNull(result);
            Assert.True(result.StatusCode == (int)HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task When_Representation_Cannot_Be_Removed_Then_InternalError_Is_Returned()
        {
            // ARRANGE
            const string identifier = "identifier";
            InitializeFakeObjects();
            _representationStoreStub.Setup(r => r.GetRepresentation(It.IsAny<string>()))
                .Returns(Task.FromResult(new Representation()));
            _apiResponseFactoryStub.Setup(a => a.CreateError(HttpStatusCode.InternalServerError, ErrorMessages.TheRepresentationCannotBeRemoved))
                .Returns(new ApiActionResult
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError
                });
            _representationStoreStub.Setup(r => r.RemoveRepresentation(It.IsAny<Representation>()))
                .Returns(Task.FromResult(false));

            // ACT
            var result = await _deleteRepresentationAction.Execute(identifier).ConfigureAwait(false);

            // ASSERT
            Assert.NotNull(result);
            Assert.True(result.StatusCode == (int)HttpStatusCode.InternalServerError);
        }

        private void InitializeFakeObjects()
        {
            _representationStoreStub = new Mock<IRepresentationStore>();
            _apiResponseFactoryStub = new Mock<IApiResponseFactory>();
            _deleteRepresentationAction = new DeleteRepresentationAction(
                _representationStoreStub.Object,
                _apiResponseFactoryStub.Object);
        }
    }
}
