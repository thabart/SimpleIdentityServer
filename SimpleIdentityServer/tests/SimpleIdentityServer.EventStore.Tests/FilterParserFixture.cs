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
        public void When_Parsing_Incorrect_Str_Then_Exception_Are_Thrown()
        {
            // ARRAGE
            InitializeFakeObjects();

            // ACTS & ASSERTS
            Assert.Throws<InvalidOperationException>(() => _parser.Parse("invalid$outer(FirstName),inner(LastName)"));
            Assert.Throws<InvalidOperationException>(() => _parser.Parse("select"));
            Assert.Throws<InvalidOperationException>(() => _parser.Parse("select$"));
        }

        [Fact]
        public void When_Parsing_Then_Instruction_Are_Returned()
        {
            // ARRAGE
            InitializeFakeObjects();

            // ACT
            var firstInstruction = _parser.Parse("join$outer(FirstName),inner(LastName)").Instruction as InnerJoinInstruction;
            var secondInstruction = _parser.Parse("select$firstName,lastName").Instruction as SelectInstruction;
            var thirdInstruction = _parser.Parse("groupby$on(FirstName|LastName)").Instruction as GroupByInstruction;
            var fourthInstruction = _parser.Parse("select$target(select$firstParameter),secondParameter").Instruction as SelectInstruction;
            var fifthInstruction = _parser.Parse("select$target(select$target(select$secondParameter)),firstParameter where$(FirstName eq thierry)").Instruction as SelectInstruction;

            // ASSERTS
            Assert.NotNull(firstInstruction);
            Assert.True(firstInstruction.GetParameter() == "outer(FirstName),inner(LastName)");
            Assert.NotNull(secondInstruction);
            Assert.True(secondInstruction.GetParameter() == "firstName,lastName");
            Assert.NotNull(thirdInstruction);
            Assert.True(thirdInstruction.GetParameter() == "on(FirstName|LastName)");
            Assert.NotNull(fourthInstruction);
            Assert.True(fourthInstruction.GetParameter() == "secondParameter");
            Assert.NotNull(fourthInstruction.GetTargetInstruction());
            Assert.True(fourthInstruction.GetTargetInstruction().GetParameter() == "firstParameter");
            Assert.NotNull(fifthInstruction);
            Assert.True(fifthInstruction.GetSubInstruction() is WhereInstruction);
            Assert.True(fifthInstruction.GetTargetInstruction() is SelectInstruction);
        }

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
            var first = _parser.Parse("groupby$on(FirstName)");
            var second = _parser.Parse("groupby$on(FirstName|LastName)");
            var firstResult = first.Execute(persons);
            var secondResult = second.Execute(persons);

            // ASSERTS
            Assert.NotNull(firstResult);
            Assert.NotNull(secondResult);
            Assert.True(firstResult.Count() == 2);
            Assert.True(secondResult.Count() == 2);
        }

        [Fact]
        public void When_Execute_GroupBy_And_Select_MinDateTime_Then_Two_DateTime_Are_Returned()
        {
            var persons = (new List<Person>
            {
                new Person
                {
                    FirstName = "thierry1",
                    LastName = "lastname",
                    BirthDate = DateTime.UtcNow
                },
                new Person
                {
                    FirstName = "thierry2",
                    LastName = "lastname",
                    BirthDate = DateTime.UtcNow.AddHours(3)
                }
            }).AsQueryable();
            
            // ARRANGE
            InitializeFakeObjects();

            // ACT
            var firstInstruction = _parser.Parse("groupby$on(LastName),aggregate(min with BirthDate)");
            var secondInstruction = _parser.Parse("groupby$on(LastName),aggregate(max with BirthDate)");
            var thirdInstruction = _parser.Parse("groupby$on(LastName),aggregate(max with BirthDate) select$FirstName");
            var firstResult = firstInstruction.Execute(persons);
            var secondResult = secondInstruction.Execute(persons);
            var thirdResult = thirdInstruction.Execute(persons);

            // ASSERTS
            Assert.NotNull(firstResult);
            Assert.NotNull(secondResult);
            Assert.NotNull(thirdResult);
            Assert.True(firstResult.Count() == 1);
            Assert.True(secondResult.Count() == 1);
            Assert.True(thirdResult.Count() == 1);
            Assert.True(firstResult.First().FirstName == "thierry1");
            Assert.True(secondResult.First().FirstName == "thierry2");
            Assert.True(thirdResult.First() == "thierry2");
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

            var res = persons.Join(persons, (fp) => fp.FirstName, (sp) => sp.LastName, (fp, sp) => new { fp.FirstName, sp }).Where(p => p.FirstName == "thierry");

            // ACT
            var firstInstruction = _parser.Parse("join$outer(FirstName),inner(LastName)");
            var secondInstruction = _parser.Parse("join$outer(FirstName),inner(LastName),select(outer$FirstName|inner) where$(outer_FirstName eq thierry)");
            var thirdInstruction = _parser.Parse("join$outer(FirstName),inner(LastName),select(outer$FirstName|inner) where$(outer_FirstName eq thierry) select$outer_FirstName");
            var firstResult = firstInstruction.Execute(persons);
            var secondResult = secondInstruction.Execute(persons);
            var thirdResult = thirdInstruction.Execute(persons);

            // ASSERTS
            Assert.NotNull(firstResult);
            Assert.True(firstResult.Count() == 2);
            Assert.NotNull(secondResult);
            Assert.True(secondResult.Count() == 2);
            Assert.NotNull(thirdResult);
            Assert.True(thirdResult.Count() == 2);
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
            var interpreter = _parser.Parse("join$target(groupby$on(Id),aggregate(min with BirthDate)),outer(Id),inner(Id),select(inner|outer$BirthDate) where(Firstname eq lastname other neq other");
            //  where$(outer_BirthDate eq inner.BirthDate)
            var result = interpreter.Execute(persons);
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
