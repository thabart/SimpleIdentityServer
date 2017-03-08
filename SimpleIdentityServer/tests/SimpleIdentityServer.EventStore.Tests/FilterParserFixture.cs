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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
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
            Assert.Throws<InvalidOperationException>(() => _parser.Parse("select$FirstName where$(FirstName eqq thierry)").Execute(persons));
            Assert.Throws<InvalidOperationException>(() => _parser.Parse("select$FirstName where$(FirstName eq thierry tot)").Execute(persons));
            Assert.Throws<InvalidOperationException>(() => _parser.Parse("select$FirstName where$(FirstName eq thierry and LastName tot Habart)").Execute(persons));
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
            var res = persons.Where(p => p.FirstName == "thierry" && p.LastName == "habart");

            InitializeFakeObjects();

            // ACT
            var instruction = _parser.Parse("select$FirstName where$(FirstName eq thierry and LastName eq lastname and LastName eq lastname)");
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
            var query = from p in persons
                        join sub in (from subP in persons
                                     group subP by subP.Id into g
                                     select new
                                     {
                                         BirthDate = g.Min(x => x.BirthDate),
                                         PersonKey = g.Key                                         
                                     })
                        on new { JoinProperty1 = p.Id, JoinProperty2 = p.BirthDate } equals new { JoinProperty1 = sub.PersonKey, JoinProperty2 = sub.BirthDate }
                        select new { p.Id, p.BirthDate, p.LastName };


            // ACT
            // join$target(groupby$on(Id),aggregate(min with BirthDate)),outer(Id|BirthDate),inner(Id|BirthDate)
            var interpreter = _parser.Parse("join$outer(Id|BirthDate),inner(Id|BirthDate) select$Id|BirthDate");
            //  where$(outer_BirthDate eq inner.BirthDate)
            var result = interpreter.Execute(persons);
            var i = result.Count();
            string s = "";
            // ASSERTS
            // Assert.NotNull(result);
            // Assert.True(result.Count() == 2);
        }

        [Fact]
        public void SaveAnonymousType()
        {
            /*
            Type pointType = null;
            AppDomain currentDom = Thread.GetDomain();
            StringBuilder asmFileNameBldr = new StringBuilder();
            asmFileNameBldr.Append("test");
            asmFileNameBldr.Append(".exe");
            string asmFileName = asmFileNameBldr.ToString();
            AssemblyName myAsmName = new AssemblyName();
            myAsmName.Name = "MyDynamicAssembly";
            AssemblyBuilder myAsmBldr = currentDom.DefineDynamicAssembly(
                               myAsmName,
                               AssemblyBuilderAccess.RunAndSave);
            // We've created a dynamic assembly space - now, we need to create a module
            // within it to reflect the type Point into.

            ModuleBuilder myModuleBldr = myAsmBldr.DefineDynamicModule(asmFileName,
                                               asmFileName);

            TypeBuilder myTypeBldr = myModuleBldr.DefineType("Point");

            FieldBuilder xField = myTypeBldr.DefineField("x", typeof(int),
                                                         FieldAttributes.Private);
            FieldBuilder yField = myTypeBldr.DefineField("y", typeof(int),
                                                         FieldAttributes.Private);

            // Build the constructor.

            Type objType = Type.GetType("System.Object");
            ConstructorInfo objCtor = objType.GetConstructor(new Type[0]);

            Type[] ctorParams = new Type[] { typeof(int), typeof(int) };
            ConstructorBuilder pointCtor = myTypeBldr.DefineConstructor(
                                       MethodAttributes.Public,
                                      CallingConventions.Standard,
                                      ctorParams);
            ILGenerator ctorIL = pointCtor.GetILGenerator();
            ctorIL.Emit(OpCodes.Ldarg_0);
            ctorIL.Emit(OpCodes.Call, objCtor);
            ctorIL.Emit(OpCodes.Ldarg_0);
            ctorIL.Emit(OpCodes.Ldarg_1);
            ctorIL.Emit(OpCodes.Stfld, xField);
            ctorIL.Emit(OpCodes.Ldarg_0);
            ctorIL.Emit(OpCodes.Ldarg_2);
            ctorIL.Emit(OpCodes.Stfld, yField);
            ctorIL.Emit(OpCodes.Ret);

            // Build the DotProduct method.

            Console.WriteLine("Constructor built.");

            MethodBuilder pointDPBldr = myTypeBldr.DefineMethod("DotProduct",
                                        MethodAttributes.Public,
                                        typeof(int),
                                        new Type[] { myTypeBldr });

            ILGenerator dpIL = pointDPBldr.GetILGenerator();
            dpIL.Emit(OpCodes.Ldarg_0);
            dpIL.Emit(OpCodes.Ldfld, xField);
            dpIL.Emit(OpCodes.Ldarg_1);
            dpIL.Emit(OpCodes.Ldfld, xField);
            dpIL.Emit(OpCodes.Mul_Ovf_Un);
            dpIL.Emit(OpCodes.Ldarg_0);
            dpIL.Emit(OpCodes.Ldfld, yField);
            dpIL.Emit(OpCodes.Ldarg_1);
            dpIL.Emit(OpCodes.Ldfld, yField);
            dpIL.Emit(OpCodes.Mul_Ovf_Un);
            dpIL.Emit(OpCodes.Add_Ovf_Un);
            dpIL.Emit(OpCodes.Ret);

            // Build the PointMain method.

            Console.WriteLine("DotProduct built.");

            MethodBuilder pointMainBldr = myTypeBldr.DefineMethod("PointMain",
                                        MethodAttributes.Public |
                                        MethodAttributes.Static,
                                        typeof(void),
                                        null);
            pointMainBldr.InitLocals = true;
            ILGenerator pmIL = pointMainBldr.GetILGenerator();

            // We have four methods that we wish to call, and must represent as
            // MethodInfo tokens:
            // - void Console.WriteLine(string)
            // - string Console.ReadLine()
            // - int Convert.Int32(string)
            // - void Console.WriteLine(string, object[])

            MethodInfo writeMI = typeof(Console).GetMethod(
                                 "Write",
                                 new Type[] { typeof(string) });
            MethodInfo readLineMI = typeof(Console).GetMethod(
                                    "ReadLine",
                                    new Type[0]);
            MethodInfo convertInt32MI = typeof(Convert).GetMethod(
                                    "ToInt32",
                                        new Type[] { typeof(string) });
            Type[] wlParams = new Type[] { typeof(string), typeof(object[]) };
            MethodInfo writeLineMI = typeof(Console).GetMethod(
                                 "WriteLine",
                                 wlParams);

            // Although we could just refer to the local variables by
            // index (short ints for Ldloc/Stloc, bytes for LdLoc_S/Stloc_S),
            // this time, we'll use LocalBuilders for clarity and to
            // demonstrate their usage and syntax.

            LocalBuilder x1LB = pmIL.DeclareLocal(typeof(int));
            LocalBuilder y1LB = pmIL.DeclareLocal(typeof(int));
            LocalBuilder x2LB = pmIL.DeclareLocal(typeof(int));
            LocalBuilder y2LB = pmIL.DeclareLocal(typeof(int));
            LocalBuilder point1LB = pmIL.DeclareLocal(myTypeBldr);
            LocalBuilder point2LB = pmIL.DeclareLocal(myTypeBldr);
            LocalBuilder tempObjArrLB = pmIL.DeclareLocal(typeof(object[]));

            pmIL.Emit(OpCodes.Ldstr, "Enter the 'x' value for point 1: ");
            pmIL.EmitCall(OpCodes.Call, writeMI, null);
            pmIL.EmitCall(OpCodes.Call, readLineMI, null);
            pmIL.EmitCall(OpCodes.Call, convertInt32MI, null);
            pmIL.Emit(OpCodes.Stloc, x1LB);

            pmIL.Emit(OpCodes.Ldstr, "Enter the 'y' value for point 1: ");
            pmIL.EmitCall(OpCodes.Call, writeMI, null);
            pmIL.EmitCall(OpCodes.Call, readLineMI, null);
            pmIL.EmitCall(OpCodes.Call, convertInt32MI, null);
            pmIL.Emit(OpCodes.Stloc, y1LB);

            pmIL.Emit(OpCodes.Ldstr, "Enter the 'x' value for point 2: ");
            pmIL.EmitCall(OpCodes.Call, writeMI, null);
            pmIL.EmitCall(OpCodes.Call, readLineMI, null);
            pmIL.EmitCall(OpCodes.Call, convertInt32MI, null);
            pmIL.Emit(OpCodes.Stloc, x2LB);

            pmIL.Emit(OpCodes.Ldstr, "Enter the 'y' value for point 2: ");
            pmIL.EmitCall(OpCodes.Call, writeMI, null);
            pmIL.EmitCall(OpCodes.Call, readLineMI, null);
            pmIL.EmitCall(OpCodes.Call, convertInt32MI, null);
            pmIL.Emit(OpCodes.Stloc, y2LB);

            pmIL.Emit(OpCodes.Ldloc, x1LB);
            pmIL.Emit(OpCodes.Ldloc, y1LB);
            pmIL.Emit(OpCodes.Newobj, pointCtor);
            pmIL.Emit(OpCodes.Stloc, point1LB);

            pmIL.Emit(OpCodes.Ldloc, x2LB);
            pmIL.Emit(OpCodes.Ldloc, y2LB);
            pmIL.Emit(OpCodes.Newobj, pointCtor);
            pmIL.Emit(OpCodes.Stloc, point2LB);

            pmIL.Emit(OpCodes.Ldstr, "({0}, {1}) . ({2}, {3}) = {4}.");
            pmIL.Emit(OpCodes.Ldc_I4_5);
            pmIL.Emit(OpCodes.Newarr, typeof(Object));
            pmIL.Emit(OpCodes.Stloc, tempObjArrLB);

            pmIL.Emit(OpCodes.Ldloc, tempObjArrLB);
            pmIL.Emit(OpCodes.Ldc_I4_0);
            pmIL.Emit(OpCodes.Ldloc, x1LB);
            pmIL.Emit(OpCodes.Box, typeof(int));
            pmIL.Emit(OpCodes.Stelem_Ref);

            pmIL.Emit(OpCodes.Ldloc, tempObjArrLB);
            pmIL.Emit(OpCodes.Ldc_I4_1);
            pmIL.Emit(OpCodes.Ldloc, y1LB);
            pmIL.Emit(OpCodes.Box, typeof(int));
            pmIL.Emit(OpCodes.Stelem_Ref);

            pmIL.Emit(OpCodes.Ldloc, tempObjArrLB);
            pmIL.Emit(OpCodes.Ldc_I4_2);
            pmIL.Emit(OpCodes.Ldloc, x2LB);
            pmIL.Emit(OpCodes.Box, typeof(int));
            pmIL.Emit(OpCodes.Stelem_Ref);

            pmIL.Emit(OpCodes.Ldloc, tempObjArrLB);
            pmIL.Emit(OpCodes.Ldc_I4_3);
            pmIL.Emit(OpCodes.Ldloc, y2LB);
            pmIL.Emit(OpCodes.Box, typeof(int));
            pmIL.Emit(OpCodes.Stelem_Ref);

            pmIL.Emit(OpCodes.Ldloc, tempObjArrLB);
            pmIL.Emit(OpCodes.Ldc_I4_4);
            pmIL.Emit(OpCodes.Ldloc, point1LB);
            pmIL.Emit(OpCodes.Ldloc, point2LB);
            pmIL.EmitCall(OpCodes.Callvirt, pointDPBldr, null);

            pmIL.Emit(OpCodes.Box, typeof(int));
            pmIL.Emit(OpCodes.Stelem_Ref);
            pmIL.Emit(OpCodes.Ldloc, tempObjArrLB);
            pmIL.EmitCall(OpCodes.Call, writeLineMI, null);

            pmIL.Emit(OpCodes.Ret);

            Console.WriteLine("PointMain (entry point) built.");

            pointType = myTypeBldr.CreateType();

            Console.WriteLine("Type completed.");

            myAsmBldr.SetEntryPoint(pointMainBldr);

            myAsmBldr.Save(asmFileName);
            */

            var dynamicAssemblyName = new AssemblyName("TempAssm");
            var assemblyBuilder = Thread.GetDomain().DefineDynamicAssembly(dynamicAssemblyName, AssemblyBuilderAccess.RunAndSave);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule("TempAssm", "TempAssm.exe");
            // var anonType = ReflectionHelper.CreateAssembly(typeof(Person), new[] { "Id", "BirthDate" }, moduleBuilder);
            // Create entry point
            TypeBuilder typeBuilder = moduleBuilder.DefineType("Program", TypeAttributes.Class | TypeAttributes.Public);
            MethodBuilder methodBuilder = typeBuilder.DefineMethod("Main", MethodAttributes.Public | MethodAttributes.Static,
                            typeof(void), null);
            methodBuilder.InitLocals = true;
            ILGenerator gen = methodBuilder.GetILGenerator();
            gen.Emit(OpCodes.Ret);
            // gen.Emit(OpCodes.Nop);
            // gen.Emit(OpCodes.Ldstr, "azeaze");
            // gen.Emit(OpCodes.Call, typeof(DateTime).GetMethod("get_Now"));
            // gen.Emit(OpCodes.Newobj, anonType.GetConstructors().First());
            // gen.Emit(OpCodes.Stloc_0);
            // gen.Emit(OpCodes.Ldloc_0);
            // gen.Emit(OpCodes.Callvirt, typeof(object).GetMethod("GetHashCode"));
            // gen.Emit(OpCodes.Pop);
            // gen.Emit(OpCodes.Ret);
            typeBuilder.CreateType();
            assemblyBuilder.SetEntryPoint(methodBuilder, PEFileKinds.ConsoleApplication);
            // File.Delete("TempAssm.exe");
            assemblyBuilder.Save("TempAssm.exe");
            string s = "";
        }

        private void InitializeFakeObjects()
        {
            _parser = new FilterParser();
        }
    }
}
