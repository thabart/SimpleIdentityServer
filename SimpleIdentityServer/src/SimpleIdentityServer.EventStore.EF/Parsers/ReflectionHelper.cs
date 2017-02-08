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

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace SimpleIdentityServer.EventStore.EF.Parsers
{
    public class ReflectionHelper
    {
        public static TypeInfo CreateNewAnonymousType<TSource>(IEnumerable<string> fieldNames)
        {
            return CreateNewAnonymousType(typeof(TSource), fieldNames);
        }

        public static TypeInfo CreateNewAnonymousType(Type sourceType, IEnumerable<string> fieldNames)
        {
            var dynamicAssemblyName = new AssemblyName("TempAssm");
            var dynamicAssembly = AssemblyBuilder.DefineDynamicAssembly(dynamicAssemblyName, AssemblyBuilderAccess.Run);
            var dynamicModule = dynamicAssembly.DefineDynamicModule("TempAssm");
            TypeBuilder dynamicAnonymousType = dynamicModule.DefineType("TempCl", TypeAttributes.Public);
            foreach (var fieldName in fieldNames)
            {
                var property = sourceType.GetProperty(fieldName);
                dynamicAnonymousType.DefineField(fieldName, property.PropertyType, FieldAttributes.Public);
            }

            return dynamicAnonymousType.CreateTypeInfo();
        }
    }
}
