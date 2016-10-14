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
using SimpleIdentityServer.Scim.Core.Apis;
using SimpleIdentityServer.Scim.Core.Models;
using SimpleIdentityServer.Scim.Core.Parsers;
using SimpleIdentityServer.Scim.Core.Stores;
using System;
using Xunit;

namespace SimpleIdentityServer.Scim.Core.Tests.Apis
{
    public class GetRepresentationActionFixture
    {
        private Mock<IRepresentationStore> _representationStoreStub;
        private Mock<IResponseParser> _responseParserStub;
        private IGetRepresentationAction _getRepresentationAction;

        [Fact]
        public void When_Passing_Null_Parameters_Then_Exceptions_Are_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACTS & ASSERTS
            Assert.Throws<ArgumentNullException>(() => _getRepresentationAction.Execute(null, null, null));
            Assert.Throws<ArgumentNullException>(() => _getRepresentationAction.Execute(string.Empty, null, null));
            Assert.Throws<ArgumentNullException>(() => _getRepresentationAction.Execute("identifier", null, null));
            Assert.Throws<ArgumentNullException>(() => _getRepresentationAction.Execute("identifier", string.Empty, null));
            Assert.Throws<ArgumentNullException>(() => _getRepresentationAction.Execute("identifier", "schema_identifier", null));
            Assert.Throws<ArgumentNullException>(() => _getRepresentationAction.Execute("identifier", "schema_identifier", string.Empty));
        }

        [Fact]
        public void When_Representation_Doesnt_Exist_Then_Null_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _representationStoreStub.Setup(r => r.GetRepresentation(It.IsAny<string>()))
                .Returns((Representation)null);

            // ACT
            var representation = _getRepresentationAction.Execute("identifier", "schema_identifier", "schema_type");

            // ASSERT
            Assert.Null(representation);
        }

        [Fact]
        public void When_Representation_Exists_The_Representation_Is_Parsed()
        {
            // ARRANGE
            var representation = new Representation();
            const string schemaId = "schema";
            const string schemaType = "type";
            InitializeFakeObjects();
            _representationStoreStub.Setup(r => r.GetRepresentation(It.IsAny<string>()))
                .Returns(representation);

            // ACT
            _getRepresentationAction.Execute("identifier", schemaId, schemaType);

            // ASSERT
            _responseParserStub.Verify(r => r.Parse(representation, schemaId, schemaType));
        }

        private void InitializeFakeObjects()
        {
            _representationStoreStub = new Mock<IRepresentationStore>();
            _responseParserStub = new Mock<IResponseParser>();
            _getRepresentationAction = new GetRepresentationAction(
                _representationStoreStub.Object,
                _responseParserStub.Object);
        }
    }
}
