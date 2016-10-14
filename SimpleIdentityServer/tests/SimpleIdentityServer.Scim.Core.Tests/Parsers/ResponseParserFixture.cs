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
using Newtonsoft.Json.Linq;
using SimpleIdentityServer.Scim.Core.DTOs;
using SimpleIdentityServer.Scim.Core.Errors;
using SimpleIdentityServer.Scim.Core.Models;
using SimpleIdentityServer.Scim.Core.Parsers;
using SimpleIdentityServer.Scim.Core.Stores;
using System;
using Xunit;

namespace SimpleIdentityServer.Scim.Core.Tests.Parsers
{
    public class ResponseParserFixture
    {
        private Mock<ISchemaStore> _schemaStoreStub;
        private IRequestParser _requestParser;
        private IResponseParser _responseParser;

        [Fact]
        public void When_Null_Or_Empty_Parameters_Are_Parsed_Then_Exceptions_Are_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACTS & ASSERTS
            Assert.Throws<ArgumentNullException>(() => _responseParser.Parse(null, null, null));
            Assert.Throws<ArgumentNullException>(() => _responseParser.Parse(new Representation(), null, null));
            Assert.Throws<ArgumentNullException>(() => _responseParser.Parse(new Representation(), string.Empty, null));
            Assert.Throws<ArgumentNullException>(() => _responseParser.Parse(new Representation(), "schema_id", null));
            Assert.Throws<ArgumentNullException>(() => _responseParser.Parse(new Representation(), "schema_id", string.Empty));
        }

        [Fact]
        public void When_Schema_Doesnt_Exist_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            const string schemaId = "schema_id";
            InitializeFakeObjects();
            _schemaStoreStub.Setup(s => s.Get(It.IsAny<string>()))
                .Returns(() => (SchemaResponse)null);

            // ACT & ASSERT
            var exception = Assert.Throws<InvalidOperationException>(() => _responseParser.Parse(new Representation(), "schema_id", "schema_type"));
            Assert.True(exception.Message == string.Format(ErrorMessages.TheSchemaDoesntExist, schemaId));
        }
        
        [Fact]
        public void When_Parsing_GroupRepresentation_Then_Response_Is_Returned()
        {
            var schemaStore = new SchemaStore();
            // ARRANGE
            InitializeFakeObjects();
            _schemaStoreStub.Setup(s => s.Get(It.IsAny<string>()))
                .Returns(schemaStore.Get(Constants.SchemaUrns.Group));
            var jObj = JObject.Parse(@"{'schemas': ['urn:ietf:params:scim:schemas:core:2.0:Group']," +
            "'displayName': 'Group A'," +
            "'members': [" +
             "{" +
               "'type': 'Group'," +
               "'value': 'bulkId:ytrewq'" +
             "}" +
           "]}");
            var result = _requestParser.Parse(jObj, Constants.SchemaUrns.Group);

            // ACT
            var response = _responseParser.Parse(result, Constants.SchemaUrns.Group, "Group");

            // ASSERT
            Assert.NotNull(response);
        }

        private void InitializeFakeObjects()
        {
            _schemaStoreStub = new Mock<ISchemaStore>();
            _requestParser = new RequestParser(_schemaStoreStub.Object);
            _responseParser = new ResponseParser(_schemaStoreStub.Object);
        }
    }
}
