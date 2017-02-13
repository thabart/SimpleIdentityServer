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
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SimpleIdentityServer.EventStore.Tests
{
    public class FilterParserFixture
    {
        private class Person
        {
            public string Id { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public DateTime BirthDate { get; set; }
            public override bool Equals(object obj)
            {
                return true;
            }
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

            // ACT
            var instruction = _parser.Parse("where$(FirstName eq thierry)");
            var result = instruction.Execute(persons);

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
            // var p2 = persons.Where(p => p.FirstName == "thierry").OrderBy(p => p.FirstName).Select(p => p.FirstName);
            // var res = persons.GroupBy(p => p.FirstName).Select(x => x.OrderBy(y => y.BirthDate)).Select(x => x.First());

            InitializeFakeObjects();

            // ACT
            var instruction = _parser.Parse("select$FirstName where$(FirstName eq thierry)");
            var result = instruction.Execute(persons);

            // ASSERTS
            Assert.NotNull(result);
            Assert.True(result.Count() == 1);
            Assert.True(result.First() == "thierry");
        }

        [Fact]
        public void When_Execute_Where_And_OrderBy_Instruction_Then_Two_Str_Are_Returned()
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

            InitializeFakeObjects();

            // ACT
            var instruction = _parser.Parse("select$FirstName where$(LastName eq lastname) orderby$(FirstName)");
            var result = instruction.Execute(persons);

            // ASSERTS
            Assert.NotNull(result);
            Assert.True(result.Count() == 2);
            Assert.True(result.First() == "laetitia");
            Assert.True(result.ElementAt(1) == "thierry");
        }

        [Fact]
        public void When_Execute_GroupBy_Instruction_Then_Records_Are_Returned()
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

            var res = persons.GroupBy(p => new { p.LastName, p.FirstName });
            ;
            // ARRANGE
            InitializeFakeObjects();

            // ACT
            // var first = _parser.Parse("groupby$on(FirstName)");
            var second = _parser.Parse("groupby$on(FirstName|LastName)");
            // var firstResult = first.Execute(persons);
            var secondResult = second.Execute(persons);

            // ASSERTS
            // Assert.NotNull(firstResult);
            Assert.NotNull(secondResult);
            var result = secondResult.Select(r => r.Key.ToString());
            // Assert.True(firstResult.Count() == 2);
            Assert.True(secondResult.Count() == 2);
        }

        [Fact]
        public void When_Execute_GroupBy_And_Select_MinDateTime_Then_Two_DateTime_Are_Returned()
        {
            var persons = (new List<Person>
            {
                new Person
                {
                    FirstName = "thierry",
                    LastName = "lastname",
                    BirthDate = DateTime.UtcNow
                },
                new Person
                {
                    FirstName = "thierry",
                    LastName = "lastname",
                    BirthDate = DateTime.UtcNow.AddHours(3)
                },
                new Person
                {
                    FirstName = "laetitia",
                    LastName = "lastname",
                    BirthDate = DateTime.UtcNow
                }
            }).AsQueryable();
            
            // ARRANGE
            InitializeFakeObjects();

            // ACT
            var instruction = _parser.Parse("groupby$on(FirstName),aggregate(min with BirthDate)");
            // instruction.Execute(persons);

            // ASSERTS
            // Assert.NotNull(result);
            // Assert.True(result.Count() == 2);
        }

        [Fact]
        public void When_Execute_Join_Instruction_Then_OneRecord_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            var persons = (new List<Person>
            {
                new Person
                {
                    FirstName = "thierry",
                    LastName = "thierry"
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

            // ACT
            var instruction = _parser.Parse("join$FirstName|LastName");
            // var result = instruction.Evaluate(persons);

            // ASSERTS
            // Assert.NotNull(result);
            // Assert.True(result.Count() == 2);
        }

        [Fact]
        public void When_Execute_Join_On_GroupBy_Then_Records_Are_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            var persons = (new List<Person>
            {
                new Person
                {
                    Id = "1",
                    BirthDate = DateTime.UtcNow,
                    FirstName = "thierry",
                    LastName = "thierry"
                },
                new Person
                {
                    Id = "1",
                    BirthDate = DateTime.UtcNow.AddDays(-1),
                    FirstName = "thierry",
                    LastName = "1"
                },
                new Person
                {
                    Id = "3",
                    FirstName = "laetitia",
                    LastName = "2"
                }
            }).AsQueryable();
            var query = from p in persons
                        join sub in (from subP in persons
                                     group subP by subP.Id into g
                                     select new
                                     {
                                         BirthDate = g.Min(x => x.BirthDate),
                                         PersonKey = g.Key                                         
                                     })
                        on p.Id equals sub.PersonKey  into rels
                        from tmp in rels
                        where p.BirthDate == tmp.BirthDate
                        select new { p.Id, p.BirthDate, p.LastName };


            // ACT
            var interpreter = _parser.Parse("join$target(groupby$on(Id),aggregate(min with BirthDate)),on(Id eq Id)");

            // interpreter.Execute(persons);
            string s = "";

            // ASSERTS
            // Assert.NotNull(result);
            // Assert.True(result.Count() == 2);
        }

        private void InitializeFakeObjects()
        {
            _parser = new FilterParser();
        }
    }
}
