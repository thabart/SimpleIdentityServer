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

using SimpleIdentityServer.EventStore.EF.Mappings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace SimpleIdentityServer.EventStore.EF.Parsers
{
    public class ReflectionResult
    {
        public AssemblyBuilder Builder;
        public TypeInfo AnonType;
    }

    public class ReflectionHelper
    {
        private static Dictionary<int, OpCode> _mappingIndiceToOpCode = new Dictionary<int, OpCode>
        {
            { 0, OpCodes.Ldarg_1 },
            { 1, OpCodes.Ldarg_2 },
            { 2, OpCodes.Ldarg_3 }
        };

        private static Dictionary<int, OpCode> _mapingStlocToOpCodes = new Dictionary<int, OpCode>
        {
            { 0, OpCodes.Stloc_0 },
            { 1, OpCodes.Stloc_1 },
            { 2, OpCodes.Stloc_2 },
            { 3, OpCodes.Stloc_3 }
        };

        public static TypeInfo CreateNewAnonymousType<TSource>(IEnumerable<string> fieldNames)
        {
            return CreateNewAnonymousType(typeof(TSource), fieldNames);
        }

        public static TypeInfo CreateNewAnonymousType(Type sourceType, IEnumerable<string> fieldNames)
        {
            return CreateNewAnonymousType(Parse(sourceType, fieldNames));
        }

        public static TypeInfo CreateNewAnonymousType(IDictionary<string, Type> dic)
        {
            return BuildAnonType(dic).AnonType;
        }

        public static AssemblyBuilder CreateAssembly<TSource>(IEnumerable<string> fieldNames)
        {
            return CreateAssembly(typeof(TSource), fieldNames);
        }

        public static AssemblyBuilder CreateAssembly<TSource>(IEnumerable<string> fieldNames, AssemblyBuilder assemblyBuilder)
        {
            return CreateAssembly(typeof(TSource), fieldNames, assemblyBuilder);
        }

        public static AssemblyBuilder CreateAssembly(Type sourceType, IEnumerable<string> fieldNames)
        {
            return BuildAnonType(Parse(sourceType, fieldNames)).Builder;
        }

        public static AssemblyBuilder CreateAssembly(Type sourceType, IEnumerable<string> fieldNames, AssemblyBuilder assemblyBuilder)
        {
            return BuildAnonType(Parse(sourceType, fieldNames), assemblyBuilder).Builder;
        }

        public static AssemblyBuilder CreateAssembly(IDictionary<string, Type> dic)
        {
            return BuildAnonType(dic).Builder;
        }

        private static ReflectionResult BuildAnonType(IDictionary<string, Type> dic, AssemblyBuilder assemblyBuilder = null)
        {
            // 1. Declare the type.
            if (assemblyBuilder == null)
            {
                var dynamicAssemblyName = new AssemblyName("TempAssm");
                assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(dynamicAssemblyName, AssemblyBuilderAccess.Run);
            }

            var objType = Type.GetType("System.Object");
            var objCtor = objType.GetConstructor(new Type[0]);
            var dynamicModule = assemblyBuilder.DefineDynamicModule("TempAssm");
            TypeBuilder dynamicAnonymousType = dynamicModule.DefineType("TempCl", TypeAttributes.Public);
            var builder = dynamicAnonymousType.DefineConstructor(MethodAttributes.Assembly, CallingConventions.Standard, dic.Select(p => p.Value).ToArray());
            var constructorIl = builder.GetILGenerator();
            var equalsMethod = dynamicAnonymousType.DefineMethod("Equals",
                MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig, CallingConventions.HasThis | CallingConventions.Standard,
                typeof(bool), new[] { typeof(object) });
            var getHashCodeMethod = dynamicAnonymousType.DefineMethod("GetHashCode",
                MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig, CallingConventions.HasThis | CallingConventions.Standard,
                typeof(int), Type.EmptyTypes);
            var ilEquals = equalsMethod.GetILGenerator();
            var ilHashCode = getHashCodeMethod.GetILGenerator();
            constructorIl.Emit(OpCodes.Ldarg_0);
            constructorIl.Emit(OpCodes.Call, objCtor);
            int i = 0,
                nbValueType = 1;
            // 2. Create the constructor & toString method.
            var objHashMethod = typeof(object).GetMethod("GetHashCode");
            foreach (var property in dic)
            {
                OpCode opCode = OpCodes.Ldarg_S;
                var kvp = _mappingIndiceToOpCode.FirstOrDefault(m => m.Key == i);
                if (!kvp.IsEmpty())
                {
                    opCode = kvp.Value;
                }

                var field = dynamicAnonymousType.DefineField($"<{property.Key}>_BackingField", property.Value, FieldAttributes.Public);
                var prop = dynamicAnonymousType.DefineProperty(property.Key, PropertyAttributes.None, property.Value, new[] { property.Value });
                var getProperty = dynamicAnonymousType.DefineMethod($"get_{property.Key}", MethodAttributes.Assembly, property.Value, null);
                var getPropertyIl = getProperty.GetILGenerator();
                // 2.1 Build constructor
                constructorIl.Emit(OpCodes.Ldarg_0);
                constructorIl.Emit(opCode);
                constructorIl.Emit(OpCodes.Stfld, field);
                // 2.2. Build property
                getPropertyIl.Emit(OpCodes.Ldarg_0);
                getPropertyIl.Emit(OpCodes.Ldfld, field);
                getPropertyIl.Emit(OpCodes.Ret);
                // 2.3 Build the get hash method
                var hashMethod = property.Value.GetMethod("GetHashCode");
                ilHashCode.Emit(OpCodes.Ldarg_0);
                ilHashCode.Emit(OpCodes.Call, getProperty);
                OpCode call = OpCodes.Callvirt;
                if (property.Value.GetTypeInfo().IsValueType)
                {
                    OpCode stLoc = OpCodes.Stloc_1;
                    /*
                    kvp = _mapingStlocToOpCodes.FirstOrDefault(m => m.Key == nbValueType);
                    if (!kvp.IsEmpty())
                    {
                        stLoc = kvp.Value;
                    }
                    */
                    var local = ilHashCode.DeclareLocal(property.Value);
                    ilHashCode.Emit(stLoc);
                    ilHashCode.Emit(OpCodes.Ldloca_S, local);
                    if (property.Value != typeof(DateTime))
                    {
                        call = OpCodes.Call;
                    }
                    else
                    {
                        hashMethod = typeof(object).GetMethod("GetHashCode");
                        ilHashCode.Emit(OpCodes.Constrained, property.Value);
                    }

                    // nbValueType++;
                }

                ilHashCode.Emit(call, hashMethod);
                if (i > 0)
                {
                    ilHashCode.Emit(OpCodes.Xor);
                }

                // 2.4 Add methods associated to the property.
                prop.SetGetMethod(getProperty);
                i++;
            }

            constructorIl.Emit(OpCodes.Ret);
            // ilHashCode.Emit(OpCodes.Ldc_I4_0);
            ilHashCode.Emit(OpCodes.Stloc_0);
            ilHashCode.Emit(OpCodes.Ldloc_0);
            ilHashCode.Emit(OpCodes.Ret);
            // Build equals method.
            var isNotNull = ilEquals.DefineLabel();
            var isNull = ilEquals.DefineLabel();
            var endOfMethod = ilEquals.DefineLabel();
            ilEquals.Emit(OpCodes.Ldc_I4_1);
            ilEquals.Emit(OpCodes.Ret);
            // ilEquals.Emit(OpCodes.Ldnull);
            // ilEquals.Emit(OpCodes.Ceq);           
            // ilEquals.Emit(OpCodes.Brtrue_S, isNull);
            // ilEquals.Emit(OpCodes.Ldc_I4_1);
            // ilEquals.Emit(OpCodes.Br_S, endOfMethod);
            // ilEquals.MarkLabel(isNull);
            // ilEquals.Emit(OpCodes.Ldc_I4_0);
            // ilEquals.Emit(OpCodes.Br_S, endOfMethod);
            // ilEquals.MarkLabel(endOfMethod);

            // var eq = typeof(object).GetMethods();
            // dynamicAnonymousType.DefineMethodOverride(equalsMethod, typeof(object).GetMethod("Equals"));
            dynamicAnonymousType.DefineMethodOverride(getHashCodeMethod, objHashMethod);
            return new ReflectionResult
            {
                AnonType = dynamicAnonymousType.CreateTypeInfo(),
                Builder = assemblyBuilder
            };
        }

        private static IDictionary<string, Type> Parse(Type sourceType, IEnumerable<string> fieldNames)
        {
            if (sourceType == null)
            {
                throw new ArgumentNullException(nameof(sourceType));
            }

            if (fieldNames == null)
            {
                throw new ArgumentNullException(nameof(fieldNames));
            }

            var kvpLst = fieldNames.Select(f =>
            {
                var property = sourceType.GetProperty(f);
                if (property == null)
                {
                    return default(KeyValuePair<string, Type>);
                }

                return new KeyValuePair<string, Type>(f, property.PropertyType);
            }).Where(f => !f.IsEmpty());
            IDictionary<string, Type> dic = new Dictionary<string, Type>();
            foreach (var kvp in kvpLst)
            {
                dic.Add(kvp);
            }

            return dic;
        }
    }
}
