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
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
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
            public int Order { get; set; }
            public int Other { get; set; }

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
            var fifthInstruction = _parser.Parse("select$target(select$target(select$secondParameter)),firstParameter where$(FirstName eq 'thierry')").Instruction as SelectInstruction;

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
        public void When_Parsing_Invalid_Where_Parameter_Then_Exception_Is_Thrown()
        {
            // ACT
            InitializeFakeObjects();
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

            // ACTS & ASSERTS
            Assert.Throws<InvalidOperationException>(() => _parser.Parse("select$FirstName where$(FirstName)").Execute(persons));
            Assert.Throws<InvalidOperationException>(() => _parser.Parse("select$FirstName where$(FirstName eq thierry)").Execute(persons));
            Assert.Throws<InvalidOperationException>(() => _parser.Parse("select$FirstName where$(FirstName eqq 'thierry')").Execute(persons));
            Assert.Throws<InvalidOperationException>(() => _parser.Parse("select$FirstName where$(FirstName eq 'thierry' tot)").Execute(persons));
            Assert.Throws<InvalidOperationException>(() => _parser.Parse("select$FirstName where$(FirstName eq 'thierry' and LastName tot 'Habart')").Execute(persons));
        }

        [Fact]
        public void When_Execute_Select_Instruction_Then_Records_Are_Returned()
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
            var firstInstruction = _parser.Parse("select$FirstName|LastName");
            var secondInstruction = _parser.Parse("select$*");
            var firstResult = firstInstruction.Execute(persons);
            var secondResult = secondInstruction.Execute(persons);

            // ASSERTS
            Assert.NotNull(firstResult);
            Assert.True(firstResult.Count() == 2);
            Assert.NotNull(secondResult);
            Assert.True(secondResult.Count() == 2);
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
                },
                new Person
                {
                    FirstName = "t a b c d",
                    LastName = "lastname"
                }
            }).AsQueryable();

            // ARRANGE
            InitializeFakeObjects();

            // ACT
            var firstInstruction = _parser.Parse("where$(FirstName eq 'thierry')");
            var secondInstruction = _parser.Parse("where$(FirstName eq 't a b c d')");
            var thirdInstruction = _parser.Parse("where$(FirstName co 't a')");
            var fourthInstruction = _parser.Parse("where$(FirstName sw 't a')");
            var fifthInstruction = _parser.Parse("where$(FirstName ew 'd')");
            var sixInstruction = _parser.Parse("where$(FirstName co 't a' or FirstName sw 'lae')");
            var firstResult = firstInstruction.Execute(persons);
            var secondResult = secondInstruction.Execute(persons);
            var thirdResult = thirdInstruction.Execute(persons);
            var fourthResult = fourthInstruction.Execute(persons);
            var fifthResult = fifthInstruction.Execute(persons);
            var sixResult = sixInstruction.Execute(persons);

            // ASSERTS
            Assert.NotNull(firstResult);
            Assert.True(firstResult.Count() == 1);
            Assert.True(firstResult.First().FirstName == "thierry");
            Assert.NotNull(secondResult);
            Assert.True(secondResult.Count() == 1);
            Assert.True(secondResult.First().FirstName == "t a b c d");
            Assert.NotNull(thirdResult);
            Assert.True(thirdResult.Count() == 1);
            Assert.True(thirdResult.First().FirstName == "t a b c d");
            Assert.NotNull(fourthResult);
            Assert.True(fourthResult.Count() == 1);
            Assert.True(fourthResult.First().FirstName == "t a b c d");
            Assert.NotNull(fifthResult);
            Assert.True(fifthResult.Count() == 1);
            Assert.True(fifthResult.First().FirstName == "t a b c d");
            Assert.NotNull(sixResult);
            Assert.True(sixResult.Count() == 2);
        }

        [Fact]
        public void When_Execute_OrderBy_Instruction_Then_Records_Are_Returned()
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
            var firstInstruction = _parser.Parse("orderby$on(FirstName)");
            var secondInstruction = _parser.Parse("orderby$on(FirstName),order(desc)");
            var firstResult = firstInstruction.Execute(persons);
            var secondResult = secondInstruction.Execute(persons);

            // ASSERTS
            Assert.NotNull(firstResult);
            Assert.True(firstResult.Count() == 2);
            Assert.True(firstResult.First().FirstName == "laetitia");
            Assert.NotNull(secondResult);
            Assert.True(secondResult.Count() == 2);
            Assert.True(secondResult.First().FirstName == "thierry");
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
            var res = persons.Where(p => p.FirstName == "thierry" && p.LastName == "habart");

            InitializeFakeObjects();

            // ACT
            var instruction = _parser.Parse("select$FirstName where$(FirstName eq 'thierry' and LastName eq 'lastname' and LastName eq 'lastname')");
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
            var instruction = _parser.Parse("select$FirstName where$(LastName eq 'lastname') orderby$on(FirstName)");
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
                    LastName = "thierry",
                    Order = 1
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

            var res = persons.Join(persons, (fp) => new { id = fp.FirstName, o = fp.Order }, (sp) => new { id = sp.FirstName, o = sp.Order }, (fp, sp) => new { fp.FirstName, sp });
            var tmp = res.ToList();

            // ACT
            var firstInstruction = _parser.Parse("join$outer(FirstName),inner(LastName)");
            var secondInstruction = _parser.Parse("join$outer(FirstName),inner(LastName),select(outer$FirstName|inner) where$(outer_FirstName eq 'thierry')");
            var thirdInstruction = _parser.Parse("join$outer(FirstName),inner(LastName),select(outer$FirstName|inner) where$(outer_FirstName eq 'thierry') select$outer_FirstName");
            var firstResult = firstInstruction.Execute(persons);
            var secondResult = secondInstruction.Execute(persons);
            var thirdResult = thirdInstruction.Execute(persons);

            // ASSERTS
            Assert.NotNull(firstResult);
            Assert.True(firstResult.Count() == 5);
            Assert.NotNull(secondResult);
            Assert.True(secondResult.Count() == 4);
            Assert.NotNull(thirdResult);
            Assert.True(thirdResult.Count() == 4);
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
                    FirstName = "thierry1",
                    LastName = "lastname1"
                },
                new Person
                {
                    Id = "1",
                    BirthDate = DateTime.UtcNow.AddDays(2),
                    FirstName = "thierry2",
                    LastName = "lastname2"
                },
                new Person
                {
                    Id = "3",
                    BirthDate = DateTime.UtcNow,
                    FirstName = "laetitia",
                    LastName = "2"
                }
            }).AsQueryable();

            // ACT
            // Link : https://github.com/machine-legacy/machine.mta/blob/master/Source/Machine.Mta.MessageInterfaces/DefaultMessageInterfaceImplementationFactory.cs
            var interpreter = _parser.Parse("join$target(groupby$on(Id),aggregate(min with BirthDate)),outer(Id|BirthDate),inner(Id|BirthDate)");
            var result = interpreter.Execute(persons);

            // ASSERTS
            Assert.NotNull(result);
            Assert.True(result.Count() == 2);
        }

        [Fact]
        public void SaveAnonymousType()
        {
            var dynamicAssemblyName = new AssemblyName("TempAssm");
            var assemblyBuilder = Thread.GetDomain().DefineDynamicAssembly(dynamicAssemblyName, AssemblyBuilderAccess.RunAndSave);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule("TempAssm.exe", "TempAssm.exe");
            var anonType = ReflectionHelper.CreateAssembly(typeof(Person), new[] { "Id", "BirthDate", "Order", "Other" }, moduleBuilder);
            // Create entry point
            TypeBuilder typeBuilder = moduleBuilder.DefineType("Program", TypeAttributes.Class | TypeAttributes.Public);
            MethodBuilder methodBuilder = typeBuilder.DefineMethod("Main", MethodAttributes.Public | MethodAttributes.Static,
                            typeof(void), null);
            methodBuilder.InitLocals = true;
            var writeMI = typeof(Console).GetMethod("Write", new Type[] { typeof(string) });

            var gen = methodBuilder.GetILGenerator();
            var dt = gen.DeclareLocal(typeof(DateTime));
            var s = gen.DeclareLocal(typeof(string));
            var order = gen.DeclareLocal(typeof(int));
            var other = gen.DeclareLocal(typeof(int));
            var i = gen.DeclareLocal(anonType);
            var r2 = gen.DeclareLocal(typeof(int));
            var r3 = gen.DeclareLocal(typeof(DateTime));
            gen.Emit(OpCodes.Call, typeof(DateTime).GetMethod("get_Now")); // Call getNow
            gen.Emit(OpCodes.Stloc_0); // Load value into first variable
            gen.Emit(OpCodes.Ldstr, "azeaze"); // create str
            gen.Emit(OpCodes.Stloc_1); // Load value into second variable
            gen.Emit(OpCodes.Ldc_I4_3); // Declare int 3
            gen.Emit(OpCodes.Stloc_2);  // Load value into third variable.
            gen.Emit(OpCodes.Ldc_I4_4); // Declare int 4
            gen.Emit(OpCodes.Stloc_3); // Load value into fourth variable
            gen.Emit(OpCodes.Ldloc_1); // Load string
            gen.Emit(OpCodes.Ldloc_0); // Load datetime
            gen.Emit(OpCodes.Ldloc_2); // Load int
            gen.Emit(OpCodes.Ldloc_3); // Load int
            gen.Emit(OpCodes.Newobj, anonType.GetConstructors().First()); // Create instance.
            gen.Emit(OpCodes.Stloc_S, i);
            gen.Emit(OpCodes.Ldloc_S, i);
            /*
            gen.Emit(OpCodes.Callvirt, anonType.GetMethod("get_BirthDate"));
            gen.Emit(OpCodes.Stloc_S, r3);
            gen.Emit(OpCodes.Ldloc_S, r3);
            gen.Emit(OpCodes.Box, typeof(DateTime));
            gen.Emit(OpCodes.Call, typeof(Console).GetMethod("Write", new Type[] { typeof(object) }));
            */
            gen.Emit(OpCodes.Callvirt, typeof(object).GetMethod("GetHashCode"));
            gen.Emit(OpCodes.Stloc_S, r2);
            gen.Emit(OpCodes.Ldloc_S, r2);
            gen.Emit(OpCodes.Call, typeof(Console).GetMethod("Write", new Type[] { typeof(int) }));
            gen.Emit(OpCodes.Ret);

            typeBuilder.CreateType();

            assemblyBuilder.SetEntryPoint(methodBuilder, PEFileKinds.ConsoleApplication);
            assemblyBuilder.Save("TempAssm.exe");
        }

        private void InitializeFakeObjects()
        {
            _parser = new FilterParser();
        }
    }
}
