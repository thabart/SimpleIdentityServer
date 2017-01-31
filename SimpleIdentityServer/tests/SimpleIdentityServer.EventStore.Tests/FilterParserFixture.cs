#region copyright
// Copyright 2017 Habart Thierry
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

using SimpleIdentityServer.EventStore.EF.Parsers;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SimpleIdentityServer.EventStore.Tests
{
    public class FilterParserFixture
    {
        private class Person
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
        }

        private FilterParser _parser;

        [Fact]
        public void When_Execute_Where_Instruction_Then_One_Record_Is_Returned()
        {
            var persons = (new List<Person>
            {
                new Person
                {
                    FirstName = "thierry",
                    LastName = "lastname"
                },
                new Person
                {
                    FirstName = "laetitia",
                    LastName = "lastname"
                }
            }).AsQueryable();

            // ARRANGE
            InitializeFakeObjects();

            var results = persons.Select(p => new { p.FirstName });
            string s2 = results.GetType().FullName;

            // ACT
            var instruction = _parser.Parse("where$(FirstName eq thierry)");
            var result = instruction.Evaluate(persons);

            // ASSERTS
            Assert.NotNull(result);
            Assert.True(result.Count() == 1);
            Assert.True(result.First().FirstName == "thierry");
        }

        [Fact]
        public void When_Execute_Where_Instruction_And_Return_First_Name_Then_Str_Is_Returned()
        {
            var persons = (new List<Person>
            {
                new Person
                {
                    FirstName = "thierry",
                    LastName = "lastname"
                },
                new Person
                {
                    FirstName = "laetitia",
                    LastName = "lastname"
                }
            }).AsQueryable();

            // ARRANGE
            InitializeFakeObjects();

            var results = persons.Select(p => new { p.FirstName });
            string s2 = results.GetType().FullName;

            // ACT
            var instruction = _parser.Parse("select$FirstName where$(FirstName eq thierry)");
            var result = instruction.Evaluate(persons);

            // ASSERTS
            Assert.NotNull(result);
            Assert.True(result.Count() == 1);
            Assert.True(result.First() == "thierry");
        }

        [Fact]
        public void When_Execute_GroupBy_Instruction_Then_Two_Records_Are_Returned()
        {
            var persons = (new List<Person>
            {
                new Person
                {
                    FirstName = "thierry",
                    LastName = "lastname"
                },
                new Person
                {
                    FirstName = "thierry",
                    LastName = "lastname"
                },
                new Person
                {
                    FirstName = "laetitia",
                    LastName = "lastname"
                }
            }).AsQueryable();
            persons.GroupBy(p => p.FirstName);

            // ARRANGE
            InitializeFakeObjects();

            var results = persons.Select(p => new { p.FirstName });
            string s2 = results.GetType().FullName;

            // ACT
            var instruction = _parser.Parse("groupby$FirstName");
            var result = instruction.Evaluate(persons);

            // ASSERTS
            Assert.NotNull(result);
            Assert.True(result.Count() == 2);
        }

        private void InitializeFakeObjects()
        {
            _parser = new FilterParser();
        }
    }
}
