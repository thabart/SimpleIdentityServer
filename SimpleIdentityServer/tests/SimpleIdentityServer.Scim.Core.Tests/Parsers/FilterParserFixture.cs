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

using Newtonsoft.Json.Linq;
using SimpleIdentityServer.Scim.Core.Parsers;
using System;
using Xunit;

namespace SimpleIdentityServer.Scim.Core.Tests.Parsers
{
    public class FilterParserFixture
    {
        private IFilterParser _filterParser;

        [Fact]
        public void When_Passing_Null_Or_Empty_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACTS & ASSERTS
            Assert.Throws<ArgumentNullException>(() => _filterParser.Parse(null));
            Assert.Throws<ArgumentNullException>(() => _filterParser.Parse(string.Empty));
        }

        [Fact]
        public void When_Parsing_One_Attribute_Then_One_Attribute_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();

            // { name : 'coucou'}
            var jObj = JObject.Parse("{name: { firstName: 'thierry' }}");

            // ACT 
            var result = _filterParser.Parse("name.firstName");

            // ASSERT
            Assert.NotNull(result);
            var filter = result as Filter;
            Assert.NotNull(filter);
            var attr = filter.Expression as AttributeExpression;
            Assert.NotNull(attr);
            Assert.NotNull(attr.Path);
            Assert.True(attr.Path.Name == "name");

            var ev = filter.Evaluate(jObj);
            string s = "";
        }

        [Fact]
        public void When_Parsing_Three_Attributes_Then_Three_Attributes_Are_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT 
            var result = _filterParser.Parse("name.firstName.firstLetter");

            // ASSERT
            Assert.NotNull(result);
            var filter = result as Filter;
            Assert.NotNull(filter);
            // Check name
            var attr = filter.Expression as AttributeExpression;
            Assert.NotNull(attr);
            Assert.NotNull(attr.Path);
            Assert.True(attr.Path.Name == "name");
            // Check firstName
            var firstName = attr.Path.Next;
            Assert.NotNull(firstName);
            Assert.NotNull(firstName.Name == "firstName");
            // Check firstLetter
            var firstLetter = firstName.Next;
            Assert.NotNull(firstLetter);
            Assert.NotNull(firstLetter.Name == "firstName");
        }

        [Fact]
        public void When_Parsing_Two_Attributes_And_Value_Filter_Then_Two_Attributes_With_Value_Filter_Are_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT 
            var result = _filterParser.Parse("name.firstName[firstLetter eq \"NO\"]");

            // ASSERT
            Assert.NotNull(result);
            var filter = result as Filter;
            Assert.NotNull(filter);
            // Check name
            var attr = filter.Expression as AttributeExpression;
            Assert.NotNull(attr);
            Assert.NotNull(attr.Path);
            Assert.True(attr.Path.Name == "name");
            // Check firstName
            var firstName = attr.Path.Next;
            Assert.NotNull(firstName);
            Assert.NotNull(firstName.Name == "firstName");
            // Check firstLetter
            var valueFilter = firstName.ValueFilter;
            Assert.NotNull(valueFilter);
            var compAttr = valueFilter.Expression as CompAttributeExpression;
            Assert.NotNull(compAttr);
            Assert.True(compAttr.Operator == ComparisonOperators.eq);
            Assert.True(compAttr.Path.Name == "firstLetter");
            Assert.True(compAttr.Value == "\"NO\"");
        }

        [Fact]
        public void When_Parsing_Two_Logical_Attributes_Then_Two_Attributes_Are_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT 
            var result = _filterParser.Parse("firstName eq thierry and lastName eq Habart");

            // ASSERT
            Assert.NotNull(result);
            var filter = result as Filter;
            Assert.NotNull(filter);
            var logicalAttr = filter.Expression as LogicalExpression;
            Assert.NotNull(logicalAttr);
            Assert.True(logicalAttr.Operator == LogicalOperators.and);
            var leftOperand = logicalAttr.AttributeLeft as CompAttributeExpression;
            var rightOperand = logicalAttr.AttributeRight as CompAttributeExpression;
            Assert.NotNull(leftOperand);
            Assert.NotNull(rightOperand);
            Assert.True(leftOperand.Operator == ComparisonOperators.eq);
            Assert.True(rightOperand.Operator == ComparisonOperators.eq);
            Assert.True(leftOperand.Path.Name == "firstName");
            Assert.True(rightOperand.Path.Name == "lastName");
        }

        private void InitializeFakeObjects()
        {
            _filterParser = new FilterParser();
        }
    }
}
