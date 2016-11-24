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
using SimpleIdentityServer.Scim.Core.Errors;
using SimpleIdentityServer.Scim.Core.Factories;
using SimpleIdentityServer.Scim.Core.Models;
using SimpleIdentityServer.Scim.Core.Parsers;
using SimpleIdentityServer.Scim.Core.Stores;
using SimpleIdentityServer.Scim.Core.Tests.Fixture;
using System;
using Xunit;

namespace SimpleIdentityServer.Scim.Core.Tests.Parsers
{
    public class RepresentationResponseParserFixture : IClassFixture<StoresFixture>
    {
        private Mock<ISchemaStore> _schemaStoreStub;
        private Mock<ICommonAttributesFactory> _commonAttributesFactoryStub;
        private IFilterParser _filterParser;
        private IRepresentationRequestParser _requestParser;
        private IRepresentationResponseParser _responseParser;
        private ISchemaStore _schemaStore;

        public RepresentationResponseParserFixture(StoresFixture stores)
        {
            _schemaStore = stores.SchemaStore;
        }

        #region Parse

        [Fact]
        public void When_Null_Or_Empty_Parameters_Are_Parsed_Then_Exceptions_Are_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACTS & ASSERTS
            Assert.Throws<ArgumentNullException>(() => _responseParser.Parse(null, "http://localhost/{id}", null, OperationTypes.Modification));
            Assert.Throws<ArgumentNullException>(() => _responseParser.Parse(new Representation(), "http://localhost/{id}", null, OperationTypes.Modification));
            Assert.Throws<ArgumentNullException>(() => _responseParser.Parse(new Representation(), "http://localhost/{id}", null, OperationTypes.Modification));
            Assert.Throws<ArgumentNullException>(() => _responseParser.Parse(new Representation(), "http://localhost/{id}", null, OperationTypes.Modification));
            Assert.Throws<ArgumentNullException>(() => _responseParser.Parse(new Representation(), "http://localhost/{id}", string.Empty, OperationTypes.Modification));
        }

        [Fact]
        public void When_Schema_Doesnt_Exist_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            const string schemaId = "schema_id";
            InitializeFakeObjects();
            _schemaStoreStub.Setup(s => s.GetSchema(It.IsAny<string>()))
                .Returns(() => (SchemaResponse)null);

            // ACT & ASSERT
            var exception = Assert.Throws<InvalidOperationException>(() => _responseParser.Parse(new Representation(), "http://localhost/{id}", "schema_id", OperationTypes.Modification));
            Assert.True(exception.Message == string.Format(ErrorMessages.TheSchemaDoesntExist, schemaId));
        }
        
        [Fact]
        public void When_Parsing_GroupRepresentation_Then_Response_Is_Returned()
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
             "}" +
           "]}");
            string error;
            var result = _requestParser.Parse(jObj, Common.Constants.SchemaUrns.Group, CheckStrategies.Strong, out error);
            _commonAttributesFactoryStub.Setup(c => c.CreateIdJson(It.IsAny<Representation>()))
                .Returns(new JProperty(Common.Constants.IdentifiedScimResourceNames.Id, "id"));
            _commonAttributesFactoryStub.Setup(c => c.CreateMetaDataAttributeJson(It.IsAny<Representation>(), It.IsAny<string>()))
                .Returns(new[] { new JProperty(Common.Constants.ScimResourceNames.Meta, "meta") });

            // ACT
            var response = _responseParser.Parse(result, "http://localhost/{id}", Common.Constants.SchemaUrns.Group, OperationTypes.Modification);

            // ASSERT
            Assert.NotNull(response);
        }

        #endregion

        #region Filter

        [Fact]
        public void When_Passing_Null_Parameter_To_Filter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(() => _responseParser.Filter(null, null));
            Assert.Throws<ArgumentNullException>(() => _responseParser.Filter(new[] { new Representation() }, null));
        }

        [Fact]
        public void When_Filtering_Groups_Then_Correct_Results_Are_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _schemaStoreStub.Setup(s => s.GetSchema(It.IsAny<string>()))
                .Returns(_schemaStore.GetSchema(Common.Constants.SchemaUrns.Group));
            var firstObj = JObject.Parse(@"{'schemas': ['urn:ietf:params:scim:schemas:core:2.0:Group']," +
            "'displayName': 'Group A'," +
            "'members': [" +
             "{" +
               "'type': 'Group1'," +
               "'value': 'bulkId:ytrewq'" +
             "}" +
           "]}");
            var secondObj = JObject.Parse(@"{'schemas': ['urn:ietf:params:scim:schemas:core:2.0:Group']," +
            "'displayName': 'Group A'," +
            "'members': [" +
             "{" +
               "'type': 'Group3'," +
               "'value': 'bulkId:ytrewq'" +
             "}" +
           "]}");
            var thirdObj = JObject.Parse(@"{'schemas': ['urn:ietf:params:scim:schemas:core:2.0:Group']," +
            "'displayName': 'Group A'," +
            "'members': [" +
             "{" +
               "'type': 'Group2'," +
               "'value': 'bulkId:ytrewq'" +
             "}" +
           "]}");
            string error;
            var firstGroup = _requestParser.Parse(firstObj, Common.Constants.SchemaUrns.Group, CheckStrategies.Strong, out error);
            var secondGroup = _requestParser.Parse(secondObj, Common.Constants.SchemaUrns.Group, CheckStrategies.Strong, out error);
            var thirdGroup = _requestParser.Parse(thirdObj, Common.Constants.SchemaUrns.Group, CheckStrategies.Strong, out error);
            var groups = new[] { firstGroup, secondGroup, thirdGroup };
            var searchOrderAscending = new SearchParameter
            {
                Filter = null,
                SortBy = _filterParser.Parse("members.type"),
                SortOrder = SortOrders.Ascending
            };
            var searchOrderDescending = new SearchParameter
            {
                Filter = null,
                SortBy = _filterParser.Parse("members.type"),
                SortOrder = SortOrders.Descending
            };
            var searchOrderAscFilterPaginate = new SearchParameter
            {
                Filter = _filterParser.Parse("displayName sw Group"),
                Count = 2,
                StartIndex = 2,
                Attributes = new [] { _filterParser.Parse("members.type") },
                SortBy = _filterParser.Parse("members.type"),
                SortOrder = SortOrders.Ascending
            };
            var searchOrderAscExcludeAttrsPaginate = new SearchParameter
            {
                Filter = _filterParser.Parse("displayName sw Group"),
                Count = 2,
                StartIndex = 2,
                ExcludedAttributes = new[] { _filterParser.Parse("members.value") },
                SortBy = _filterParser.Parse("members.type"),
                SortOrder = SortOrders.Ascending
            };

            // ACTS
            var ascendingResult = _responseParser.Filter(groups, searchOrderAscending);
            var descendingResult = _responseParser.Filter(groups, searchOrderDescending);
            var filteredResult = _responseParser.Filter(groups, searchOrderAscFilterPaginate);
            var secondFilteredResult = _responseParser.Filter(groups, searchOrderAscExcludeAttrsPaginate);

            // ASSERTS
            Assert.NotNull(ascendingResult);
            Assert.NotNull(descendingResult);
            Assert.NotNull(filteredResult);
            Assert.NotNull(secondFilteredResult);
        }

        #endregion

        private void InitializeFakeObjects()
        {
            _schemaStoreStub = new Mock<ISchemaStore>();
            _commonAttributesFactoryStub = new Mock<ICommonAttributesFactory>();
            _filterParser = new FilterParser();
            _requestParser = new RepresentationRequestParser(_schemaStoreStub.Object);
            _responseParser = new RepresentationResponseParser(_schemaStoreStub.Object, _commonAttributesFactoryStub.Object);
        }
    }
}
