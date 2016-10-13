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
using SimpleIdentityServer.Scim.Core.Parsers;
using SimpleIdentityServer.Scim.Core.Stores;
using Xunit;

namespace SimpleIdentityServer.Scim.Core.Tests.Parsers
{
    public class ResponseParserFixture
    {
        private Mock<ISchemaStore> _schemaStoreStub;
        private IResponseParser _responseParser;
        private IRequestParser _requestParser;

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
            var response = _responseParser.Parse(result, Constants.SchemaUrns.Group);

            // ASSERT
            Assert.NotNull(response);
        }

        private void InitializeFakeObjects()
        {
            _schemaStoreStub = new Mock<ISchemaStore>();
            _responseParser = new ResponseParser(_schemaStoreStub.Object);
            _requestParser = new RequestParser(_schemaStoreStub.Object);
        }
    }
}
