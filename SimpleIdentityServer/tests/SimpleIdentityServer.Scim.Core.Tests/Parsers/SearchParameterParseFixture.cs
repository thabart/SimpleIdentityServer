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

using System.Linq;
using Moq;
using SimpleIdentityServer.Scim.Core.Parsers;
using Xunit;
using Microsoft.AspNetCore.WebUtilities;
using System;
using Microsoft.AspNetCore.Http.Internal;
using SimpleIdentityServer.Scim.Core.Errors;
using Newtonsoft.Json.Linq;

namespace SimpleIdentityServer.Scim.Core.Tests.Parsers
{
    public class SearchParameterParseFixture
    {
        private Mock<IFilterParser> _filterParserStub;
        private ISearchParameterParser _searchParameterParser;

        [Fact]
        public void When_Passing_No_Query_Or_Json_Then_Empty_Search_Parameter_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACTS & ASSERTS
            Assert.NotNull(_searchParameterParser.ParseQuery(null));
            Assert.NotNull(_searchParameterParser.ParseJson(null));
        }

        [Fact]
        public void When_Parsing_Invalid_Query_Parameters_Then_Exceptions_Are_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var invalidIndexQuery = QueryHelpers.ParseQuery("attributes=att1&attributes=att2&startIndex=invalidIndex");
            var invalidCountQuery = QueryHelpers.ParseQuery("attributes=att1&attributes=att2&startIndex=2&count=invalidCount");
            var invalidSortOrderQuery = QueryHelpers.ParseQuery("attributes=att1&attributes=att2&startIndex=2&count=3&sortOrder=invalid");
            var invalidFilterQuery = QueryHelpers.ParseQuery("attributes=att1&attributes=att2&startIndex=2&count=3&sortOrder=ascending&filter=invalid");
            _filterParserStub.Setup(f => f.Parse("invalid"))
                .Returns((Filter)null);

            // ACTS
            var invalidIndexException= Assert.Throws<InvalidOperationException>(() => (_searchParameterParser.ParseQuery(new QueryCollection(invalidIndexQuery))));
            var invalidCountException = Assert.Throws<InvalidOperationException>(() => (_searchParameterParser.ParseQuery(new QueryCollection(invalidCountQuery))));
            var invalidSortOrderException = Assert.Throws<InvalidOperationException>(() => (_searchParameterParser.ParseQuery(new QueryCollection(invalidSortOrderQuery))));
            var invalidFilterException = Assert.Throws<InvalidOperationException>(() => (_searchParameterParser.ParseQuery(new QueryCollection(invalidFilterQuery))));

            // ASSERTS
            Assert.NotNull(invalidIndexException);
            Assert.NotNull(invalidCountException);
            Assert.NotNull(invalidSortOrderException);
            Assert.NotNull(invalidFilterException);
            Assert.True(invalidIndexException.Message == string.Format(ErrorMessages.TheParameterIsNotValid, "startIndex"));
            Assert.True(invalidCountException.Message == string.Format(ErrorMessages.TheParameterIsNotValid, "count"));
            Assert.True(invalidSortOrderException.Message == string.Format(ErrorMessages.TheParameterIsNotValid, "sortOrder"));
            Assert.True(invalidFilterException.Message == string.Format(ErrorMessages.TheParameterIsNotValid, "filter"));
        }

        [Fact]
        public void When_Parsing_Valid_Query_Parameters_Then_Search_Parameters_Are_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            var filter = new Filter();
            var sortByFilder = new Filter();
            var att1Filter = new Filter();
            var att2Filter = new Filter();
            var query = QueryHelpers.ParseQuery("attributes=att1&attributes=att2&excludedAttributes=ex1&sortBy=name&startIndex=2&count=3&sortOrder=ascending&filter=filter");
            _filterParserStub.Setup(f => f.Parse("filter"))
                .Returns(filter);
            _filterParserStub.Setup(f => f.Parse("name"))
                .Returns(sortByFilder);
            _filterParserStub.Setup(f => f.Parse("att1"))
                .Returns(att1Filter);
            _filterParserStub.Setup(f => f.Parse("att2"))
                .Returns(att2Filter);

            // ACT
            var result = _searchParameterParser.ParseQuery(new QueryCollection(query));

            // ASSERTS
            Assert.NotNull(result);
            Assert.True(result.Attributes.All(r => new[] { att1Filter, att2Filter }.Contains(r)));
            Assert.True(result.ExcludedAttributes.All(r => new[] { "ex1" }.Contains(r)));
            Assert.True(result.SortBy == sortByFilder);
            Assert.True(result.StartIndex == 2);
            Assert.True(result.Count == 3);
            Assert.True(result.SortOrder == SortOrders.Ascending);
            Assert.True(result.Filter == filter);
        }

        [Fact]
        public void When_Parsing_Invalid_Json_Parameters_Then_Exceptions_Are_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var invalidIndexJson = JObject.Parse("{attributes:['att1','att2'], startIndex:'invalidIndex'}");
            var invalidCountJson = JObject.Parse("{attributes:['att1','att2'], startIndex: 2, count:'invalidCount'}");
            var invalidSortOrderJson = JObject.Parse("{attributes:['att1','att2'], startIndex:2, count: 3, sortOrder:'invalid'}");
            var invalidFilterJson = JObject.Parse("{attributes:['att1','att2'], startIndex:2, count:3, sortOrder:'ascending', filter:'invalid'}");
            _filterParserStub.Setup(f => f.Parse("invalid"))
                .Returns((Filter)null);

            // ACTS
            var invalidIndexException = Assert.Throws<InvalidOperationException>(() => (_searchParameterParser.ParseJson(invalidIndexJson)));
            var invalidCountException = Assert.Throws<InvalidOperationException>(() => (_searchParameterParser.ParseJson(invalidCountJson)));
            var invalidSortOrderException = Assert.Throws<InvalidOperationException>(() => (_searchParameterParser.ParseJson(invalidSortOrderJson)));
            var invalidFilterException = Assert.Throws<InvalidOperationException>(() => (_searchParameterParser.ParseJson(invalidFilterJson)));

            // ASSERTS
            Assert.NotNull(invalidIndexException);
            Assert.NotNull(invalidCountException);
            Assert.NotNull(invalidSortOrderException);
            Assert.NotNull(invalidFilterException);
            Assert.True(invalidIndexException.Message == string.Format(ErrorMessages.TheParameterIsNotValid, "startIndex"));
            Assert.True(invalidCountException.Message == string.Format(ErrorMessages.TheParameterIsNotValid, "count"));
            Assert.True(invalidSortOrderException.Message == string.Format(ErrorMessages.TheParameterIsNotValid, "sortOrder"));
            Assert.True(invalidFilterException.Message == string.Format(ErrorMessages.TheParameterIsNotValid, "filter"));
        }

        [Fact]
        public void When_Parsing_Valid_Json_Parameters_Then_Search_Parameters_Are_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            var filter = new Filter();
            var sortByFilder = new Filter();
            var att1Filter = new Filter();
            var att2Filter = new Filter();
            var json = JObject.Parse("{attributes:['att1','att2'], excludedAttributes: ['ex1'], startIndex:2, count:3, sortBy:'name', sortOrder:'ascending', filter:'filter'}");
            _filterParserStub.Setup(f => f.Parse("filter"))
                .Returns(filter);
            _filterParserStub.Setup(f => f.Parse("name"))
                .Returns(sortByFilder);
            _filterParserStub.Setup(f => f.Parse("att1"))
                .Returns(att1Filter);
            _filterParserStub.Setup(f => f.Parse("att2"))
                .Returns(att2Filter);

            // ACT
            var result = _searchParameterParser.ParseJson(json);

            // ASSERTS
            Assert.NotNull(result);
            Assert.True(result.Attributes.All(r => new[] { att1Filter, att2Filter }.Contains(r)));
            Assert.True(result.ExcludedAttributes.All(r => new[] { "ex1" }.Contains(r)));
            Assert.True(result.SortBy == sortByFilder);
            Assert.True(result.StartIndex == 2);
            Assert.True(result.Count == 3);
            Assert.True(result.SortOrder == SortOrders.Ascending);
            Assert.True(result.Filter == filter);
        }

        private void InitializeFakeObjects()
        {
            _filterParserStub = new Mock<IFilterParser>();
            _searchParameterParser = new SearchParameterParser(_filterParserStub.Object);
        }
    }
}
