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
using SimpleIdentityServer.Scim.Common.DTOs;
using SimpleIdentityServer.Scim.Common.Models;
using SimpleIdentityServer.Scim.Core.Errors;
using SimpleIdentityServer.Scim.Core.Parsers;
using SimpleIdentityServer.Scim.Core.Stores;
using SimpleIdentityServer.Scim.Core.Tests.Fixture;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Scim.Core.Tests.Parsers
{
    public class RepresentationRequestParserFixture : IClassFixture<StoresFixture>
    {
        private Mock<ISchemaStore> _schemaStoreStub;
        private IRepresentationRequestParser _requestParser;
        private ISchemaStore _schemaStore;
        private IJsonParser _jsonParser;

        public RepresentationRequestParserFixture(StoresFixture stores)
        {
            _schemaStore = stores.SchemaStore;
        }

        #region Parse

        [Fact]
        public async Task When_Passing_Null_Parameters_Then_Exceptions_Are_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACTS & ASSERTS
            await Assert.ThrowsAsync<ArgumentNullException>(() => _requestParser.Parse(null, null, CheckStrategies.Strong));
            await Assert.ThrowsAsync<ArgumentNullException>(() => _requestParser.Parse(new JObject(), null, CheckStrategies.Strong));
        }

        [Fact]
        public async Task When_Schema_Doesnt_Exist_Then_Null_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _schemaStoreStub.Setup(s => s.GetSchema(It.IsAny<string>()))
                .Returns(Task.FromResult((SchemaResponse)null));

            // ACT
            var result = await _requestParser.Parse(new JObject(), "invalid", CheckStrategies.Strong);

            // ASSERT
            Assert.NotNull(result);
            Assert.True(result.ErrorMessage == string.Format(ErrorMessages.TheSchemaDoesntExist, "invalid"));
        }

        [Fact]
        public async Task When_Expecting_Array_But_Its_Singular_Then_Null_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _schemaStoreStub.Setup(s => s.GetSchema(It.IsAny<string>()))
                .Returns(_schemaStore.GetSchema(Common.Constants.SchemaUrns.Group));
            var jObj = JObject.Parse(@"{'schemas': ['urn:ietf:params:scim:schemas:core:2.0:Group']," +
            "'displayName': 'Group A'," +
            "'members': 'members'" +
           "}");

            // ACT
            var result = await _requestParser.Parse(jObj, Common.Constants.SchemaUrns.Group, CheckStrategies.Strong);

            // ASSERT
            Assert.NotNull(result);
            Assert.True(result.ErrorMessage == string.Format(ErrorMessages.TheAttributeIsNotAnArray, "members"));
        }
        
        [Fact]
        public async Task When_Expecting_Boolean_But_Its_A_String_Then_Null_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _schemaStoreStub.Setup(s => s.GetSchema(It.IsAny<string>()))
                .Returns(_schemaStore.GetSchema(Common.Constants.SchemaUrns.User));
            var jObj = JObject.Parse(@"{'schemas': ['urn:ietf:params:scim:schemas:core:2.0:User']," +
            "'externalId': 'bjensen'," +
            "'userName': 'bjensen'," +
            "'name': {" +
                "'familyName': 'Jensen'," +
                "'givenName':'Barbara'" +
            "},"+
            "'active': 'active'}");

            // ACT
            var result = await _requestParser.Parse(jObj, Common.Constants.SchemaUrns.User, CheckStrategies.Strong);

            // ASSERTS
            Assert.NotNull(result);
            Assert.True(result.ErrorMessage == string.Format(ErrorMessages.TheAttributeTypeIsNotCorrect, "active", Common.Constants.SchemaAttributeTypes.Boolean));
        }

        [Fact]
        public async Task When_Simple_PostGroupRequest_Is_Parsed_Then_CorrectValues_Are_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _schemaStoreStub.Setup(s => s.GetSchema(It.IsAny<string>()))
                .Returns(_schemaStore.GetSchema(Common.Constants.SchemaUrns.Group));
            var jObj = JObject.Parse(@"{'schemas': ['urn:ietf:params:scim:schemas:core:2.0:Group'],"+
            "'displayName': 'Group A',"+
            "'members': ["+
             "{"+
               "'type': 'Group',"+
               "'value': 'bulkId:ytrewq'"+
             "}"+
           "]}");

            // ACT
            var result = await _requestParser.Parse(jObj, Common.Constants.SchemaUrns.Group, CheckStrategies.Strong);

            // ASSERTS
            Assert.NotNull(result);
            Assert.True(result.Representation.Attributes.Any(s => s.SchemaAttribute.Name == "displayName"));
            var members = result.Representation.Attributes.First(s => s.SchemaAttribute.Name == "members") as ComplexRepresentationAttribute;
            Assert.NotNull(members);
            Assert.True(members.SchemaAttribute.Name == "members");
            Assert.True(members.Values.Count() == 1);
            var value = members.Values.First() as ComplexRepresentationAttribute;
            Assert.True(value.Values.Any(v =>
            {
                var singularAttribute = v as SingularRepresentationAttribute<string>;
                if (singularAttribute == null)
                {
                    return false;
                }

                return new[] { "type", "value" }.Contains(singularAttribute.SchemaAttribute.Name) && new[] { "Group", "bulkId:ytrewq" }.Contains(singularAttribute.Value);
            }));
        }

        [Fact]
        public async Task When_Complex_PostGroupRequest_Is_Parsed_Then_CorrectValues_Are_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _schemaStoreStub.Setup(s => s.GetSchema(It.IsAny<string>()))
                .Returns(_schemaStore.GetSchema(Common.Constants.SchemaUrns.Group));
            var jObj = JObject.Parse(@"{'schemas': ['urn:ietf:params:scim:schemas:core:2.0:Group']," +
            "'displayName': 'Group A'," +
            "'members': [" +
             "{" +
               "'type': 'Group'," +
               "'value': 'bulkId:ytrewq'" +
             "}," +
             "{" +
               "'type': 'Group'," +
               "'value': 'bulkId:ytrewq'" +
             "}," +
             "{" +
               "'type': 'Group'," +
               "'value': 'bulkId:ytrewq'" +
             "}" +
           "]}");

            // ACT
            var result = await _requestParser.Parse(jObj, Common.Constants.SchemaUrns.Group, CheckStrategies.Strong);


            // ASSERTS
            Assert.NotNull(result);
            Assert.True(result.Representation.Attributes.Any(s => s.SchemaAttribute.Name == "displayName"));
            var members = result.Representation.Attributes.First(s => s.SchemaAttribute.Name == "members") as ComplexRepresentationAttribute;
            Assert.NotNull(members);
            Assert.True(members.SchemaAttribute.Name == "members");
            Assert.True(members.Values.Count() == 3);
            var value = members.Values.First() as ComplexRepresentationAttribute;
            Assert.True(value.Values.Any(v =>
            {
                var singularAttribute = v as SingularRepresentationAttribute<string>;
                if (singularAttribute == null)
                {
                    return false;
                }

                return new[] { "type", "value" }.Contains(singularAttribute.SchemaAttribute.Name) && new[] { "Group", "bulkId:ytrewq" }.Contains(singularAttribute.Value);
            }));
        }

        [Fact]
        public async Task When_Simple_PostUserRequest_Is_Parsed_Then_CorrectValues_Are_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _schemaStoreStub.Setup(s => s.GetSchema(It.IsAny<string>()))
                .Returns(_schemaStore.GetSchema(Common.Constants.SchemaUrns.User));
            var jObj = JObject.Parse(@"{'schemas': ['urn:ietf:params:scim:schemas:core:2.0:User']," +
            "'externalId': 'bjensen'," +
            "'userName': 'bjensen'," +
            "'name': {" +
                "'familyName': 'Jensen',"+
                "'givenName':'Barbara'" +
            "}}");

            // ACT
            var result = await _requestParser.Parse(jObj, Common.Constants.SchemaUrns.User, CheckStrategies.Strong);


            // ASSERTS
            Assert.NotNull(result);
            Assert.True(result.Representation.Attributes.Any());
        }

        #endregion

        private void InitializeFakeObjects()
        {
            _schemaStoreStub = new Mock<ISchemaStore>();
            var parser = new RepresentationRequestParser(_schemaStoreStub.Object);
            _requestParser = parser;
            _jsonParser = parser;
        }
    }
}
