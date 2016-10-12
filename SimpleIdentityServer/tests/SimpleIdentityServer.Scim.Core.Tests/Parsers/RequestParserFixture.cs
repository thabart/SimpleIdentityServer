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
using SimpleIdentityServer.Scim.Core.Parsers;
using SimpleIdentityServer.Scim.Core.Stores;
using System;
using Xunit;

namespace SimpleIdentityServer.Scim.Core.Tests.Parsers
{
    public class RequestParserFixture
    {
        private Mock<ISchemaStore> _schemaStoreStub;

        private IRequestParser _requestParser;

        [Fact]
        public void When_Passing_Null_Parameters_Then_Exceptions_Are_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACTS & ASSERTS
            Assert.Throws<ArgumentNullException>(() => _requestParser.Parse(null, null));
            Assert.Throws<ArgumentNullException>(() => _requestParser.Parse(new JObject(), null));
        }

        [Fact]
        public void When_Schema_Doesnt_Exist_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            _schemaStoreStub.Setup(s => s.Get(It.IsAny<string>()))
                .Returns((SchemaResponse)null);

            // ACT & ASSERT
            var exception = Assert.Throws<InvalidOperationException>(() => _requestParser.Parse(new JObject(), "invalid"));
        }

        [Fact]
        public void When_PostGroupRequest_Is_Parsed_Then_SmtgHappened()
        {
            var schemaStore = new SchemaStore();
            // ARRANGE
            InitializeFakeObjects();
            _schemaStoreStub.Setup(s => s.Get(It.IsAny<string>()))
                .Returns(schemaStore.Get(Constants.SchemaUrns.Group));
            var jObj = JObject.Parse(@"{'schemas': ['urn:ietf:params:scim:schemas:core:2.0:Group'],"+
            "'displayName': 'Group A',"+
            "'members': ["+
             "{"+
               "'type': 'Group',"+
               "'value': 'bulkId:ytrewq'"+
             "}"+
           "]}");

            // ACT
            _requestParser.Parse(jObj, Constants.SchemaUrns.Group);
        }

        private void InitializeFakeObjects()
        {
            _schemaStoreStub = new Mock<ISchemaStore>();
            _requestParser = new RequestParser(_schemaStoreStub.Object);
        }
    }
}
