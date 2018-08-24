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
// limitations under the License.C:\Projects\SimpleIdentityServer\SimpleIdentityServer\src\SimpleIdentityServer.Scim.Core\DTOs\
#endregion

using Moq;
using SimpleIdentityServer.Scim.Common.Models;
using SimpleIdentityServer.Scim.Core.Apis;
using SimpleIdentityServer.Scim.Core.Errors;
using SimpleIdentityServer.Scim.Core.Factories;
using SimpleIdentityServer.Scim.Core.Parsers;
using SimpleIdentityServer.Scim.Core.Results;
using SimpleIdentityServer.Scim.Core.Stores;
using SimpleIdentityServer.Scim.Core.Validators;
using System;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Scim.Core.Tests.Apis
{
    public class GetRepresentationActionFixture
    {
        private Mock<IRepresentationStore> _representationStoreStub;
        private Mock<IRepresentationResponseParser> _responseParserStub;
        private Mock<IApiResponseFactory> _apiResponseFactoryStub;
        private Mock<IParametersValidator> _parametersValidatorStub;
        private IGetRepresentationAction _getRepresentationAction;

        [Fact]
        public async Task When_Passing_Null_Parameters_Then_Exceptions_Are_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACTS & ASSERTS
            await Assert.ThrowsAsync<ArgumentNullException>(() => _getRepresentationAction.Execute(null, "http://location/{id}", null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => _getRepresentationAction.Execute(string.Empty, "http://location/{id}", null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => _getRepresentationAction.Execute("identifier", "http://location/{id}", null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => _getRepresentationAction.Execute("identifier", "http://location/{id}", string.Empty));
        }

        [Fact]
        public async Task When_Representation_Doesnt_Exist_Then_404_Code_Is_Returned()
        {
            const string identifier = "identifier";
            // ARRANGE
            InitializeFakeObjects();
            _representationStoreStub.Setup(r => r.GetRepresentation(It.IsAny<string>()))
                .Returns(Task.FromResult((Representation)null));
            _apiResponseFactoryStub.Setup(f => f.CreateError(HttpStatusCode.NotFound, string.Format(ErrorMessages.TheResourceDoesntExist, identifier)))
                .Returns(new ApiActionResult
                {
                    StatusCode = (int)HttpStatusCode.NotFound
                });
                

            // ACT
            var result = await _getRepresentationAction.Execute("identifier", "http://location/{id}", "schema_identifier");

            // ASSERT
            Assert.NotNull(result);
            Assert.True(result.StatusCode == (int)HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task When_Representation_Exists_The_Representation_Is_Parsed_And_200_Is_Returned()
        {
            // ARRANGE
            var representation = new Representation
            {
                Id = "id"
            };
            const string schemaId = "schema";
            const string location = "http://location/{id}";
            InitializeFakeObjects();
            _representationStoreStub.Setup(r => r.GetRepresentation(It.IsAny<string>()))
                .Returns(Task.FromResult(representation));
            _responseParserStub.Setup(r => r.Parse(representation, location.Replace("{id}", representation.Id), schemaId, OperationTypes.Query))
                .Returns(Task.FromResult(new Response
                {
                    Location = location
                }));

            // ACT
            await _getRepresentationAction.Execute("identifier", location, schemaId);

            // ASSERT
            _responseParserStub.Verify(r => r.Parse(representation, location.Replace("{id}", representation.Id), schemaId, OperationTypes.Query));
            _apiResponseFactoryStub.Verify(a => a.CreateResultWithContent(HttpStatusCode.OK, It.IsAny<object>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));
        }

        private void InitializeFakeObjects()
        {
            _representationStoreStub = new Mock<IRepresentationStore>();
            _responseParserStub = new Mock<IRepresentationResponseParser>();
            _apiResponseFactoryStub = new Mock<IApiResponseFactory>();
            _parametersValidatorStub = new Mock<IParametersValidator>();
            _getRepresentationAction = new GetRepresentationAction(
                _representationStoreStub.Object,
                _responseParserStub.Object,
                _apiResponseFactoryStub.Object,
                _parametersValidatorStub.Object);
        }
    }
}
