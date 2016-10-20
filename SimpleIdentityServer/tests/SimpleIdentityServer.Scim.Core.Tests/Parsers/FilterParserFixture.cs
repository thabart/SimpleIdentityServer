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
using SimpleIdentityServer.Scim.Core.Parsers;
using Xunit;

namespace SimpleIdentityServer.Scim.Core.Tests.Parsers
{
    public class FilterParserFixture
    {
        private IFilterParser _filterParser;

        [Fact]
        public void When_Parsing_Single_Attribute_Then_One_Attribute_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT 
            var result = _filterParser.Parse("name");

            // ASSERT
            Assert.NotNull(result);
            var filter = result as Filter;
            Assert.NotNull(filter);
        }

        [Fact]
        public void When_Parsing_LogicalAttribute_Then_One_Attribute_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT 
            var result = _filterParser.Parse("meta.resourceType.name1[name.name eq 5] eq User and meta.resourceType.name2 eq User and meta.resourceType.name3 eq User or meta.resourceType.name4 eq User");

            // ASSERT
            Assert.NotNull(result);
            var filter = result as Filter;
            Assert.NotNull(filter);
        }

        private void InitializeFakeObjects()
        {
            _filterParser = new FilterParser();
        }
    }
}
