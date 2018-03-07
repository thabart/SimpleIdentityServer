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
    public class ReflectionHelper
    {
        private static Dictionary<int, OpCode> _mappingIndiceToOpCode = new Dictionary<int, OpCode>
        {
            { 0, OpCodes.Ldarg_1 },
            { 1, OpCodes.Ldarg_2 },
            { 2, OpCodes.Ldarg_3 }
        };

        private static Dictionary<int, OpCode> _mappingStlocToOpCodes = new Dictionary<int, OpCode>
        {
            { 0, OpCodes.Stloc_0 },
            { 1, OpCodes.Stloc_1 },
            { 2, OpCodes.Stloc_2 },
            { 3, OpCodes.Stloc_3 }
        };

        private static Dictionary<int, OpCode> _mappingLdlocToOpCodes = new Dictionary<int, OpCode>
        {
            { 0, OpCodes.Ldloc_0 },
            { 1, OpCodes.Ldloc_1 },
            { 2, OpCodes.Ldloc_2 },
            { 3, OpCodes.Ldloc_3 }
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
            return BuildAnonType(dic);
        }

        public static TypeInfo CreateAssembly<TSource>(IEnumerable<string> fieldNames)
        {
            return CreateAssembly(typeof(TSource), fieldNames);
        }

        public static TypeInfo CreateAssembly<TSource>(IEnumerable<string> fieldNames, ModuleBuilder moduleBuilder)
        {
            return CreateAssembly(typeof(TSource), fieldNames, moduleBuilder);
        }

        public static TypeInfo CreateAssembly(Type sourceType, IEnumerable<string> fieldNames)
        {
            return BuildAnonType(Parse(sourceType, fieldNames));
        }

        public static TypeInfo CreateAssembly(Type sourceType, IEnumerable<string> fieldNames, ModuleBuilder moduleBuilder)
        {
            return BuildAnonType(Parse(sourceType, fieldNames), moduleBuilder);
        }

        public static TypeInfo CreateAssembly(IDictionary<string, Type> dic)
        {
            return BuildAnonType(dic);
        }

        private static TypeInfo BuildAnonType(IDictionary<string, Type> dic, ModuleBuilder moduleBuilder = null)
        {
            // 1. Declare the type.
            if (moduleBuilder == null)
            {
                var dynamicAssemblyName = new AssemblyName("TempAssm");
                var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(dynamicAssemblyName, AssemblyBuilderAccess.Run);
                moduleBuilder = assemblyBuilder.DefineDynamicModule("TempAssm");
            }

            TypeBuilder dynamicAnonymousType = moduleBuilder.DefineType("TempCl", TypeAttributes.Public);
            var objType = Type.GetType("System.Object");
            var objCtor = objType.GetConstructor(new Type[0]);
            var builder = dynamicAnonymousType.DefineConstructor(MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName
                , CallingConventions.Standard, dic.Select(p => p.Value).ToArray());
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
            ilHashCode.DeclareLocal(typeof(int));
            ilHashCode.Emit(OpCodes.Nop);
            // var hashResult = ilHashCode.DeclareLocal(typeof(int));
            int i = 0;
            bool hasState = false;
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

                var field = dynamicAnonymousType.DefineField($"<{property.Key}>_BackingField", property.Value, FieldAttributes.Private);
                var prop = dynamicAnonymousType.DefineProperty(property.Key, PropertyAttributes.None, property.Value, new[] { property.Value });
                var getProperty = dynamicAnonymousType.DefineMethod($"get_{property.Key}", MethodAttributes.HideBySig | MethodAttributes.Public | MethodAttributes.SpecialName, property.Value, null);
                var setProperty = dynamicAnonymousType.DefineMethod($"set_{property.Key}", MethodAttributes.HideBySig | MethodAttributes.Public | MethodAttributes.SpecialName, null, new[] { property.Value });
                var getPropertyIl = getProperty.GetILGenerator();
                var setPropertyIL = setProperty.GetILGenerator();
                // 2.1 Build constructor
                constructorIl.Emit(OpCodes.Ldarg_0);
                if (opCode == OpCodes.Ldarg_S)
                {
                    constructorIl.Emit(opCode, i + 1);
                }
                else
                {
                    constructorIl.Emit(opCode);
                }

                constructorIl.Emit(OpCodes.Call, setProperty);
                // 2.2. Build get property
                getPropertyIl.Emit(OpCodes.Ldarg_0);
                getPropertyIl.Emit(OpCodes.Ldfld, field);
                getPropertyIl.Emit(OpCodes.Ret);
                // 2.3 Build set property
                setPropertyIL.Emit(OpCodes.Ldarg_0);
                setPropertyIL.Emit(OpCodes.Ldarg_1);
                setPropertyIL.Emit(OpCodes.Stfld, field);
                setPropertyIL.Emit(OpCodes.Ret);
                // 2.3 Build the get hash method
                var hashMethod = property.Value.GetMethod("GetHashCode", new Type[0]);
                if (hashMethod == null)
                {
                    hashMethod = typeof(object).GetMethod("GetHashCode", new Type[0]);
                }

                ilHashCode.Emit(OpCodes.Ldarg_0);
                if (field.FieldType.GetTypeInfo().IsArray)
                {
                    ilHashCode.Emit(OpCodes.Ldfld, field);
                    // TODO : Support this hash code.
                }
                if (field.FieldType.GetTypeInfo().IsValueType)
                {
                    if (field.FieldType.GetTypeInfo().IsEnum)
                    {
                        ilHashCode.Emit(OpCodes.Ldfld, field);
                        ilHashCode.Emit(OpCodes.Box, field.FieldType);
                        ilHashCode.Emit(OpCodes.Callvirt, hashMethod);
                    }
                    else
                    {
                        ilHashCode.Emit(OpCodes.Ldflda, field);
                        ilHashCode.Emit(OpCodes.Call, hashMethod);
                    }
                }
                else
                {
                    ilHashCode.Emit(OpCodes.Ldfld, field);
                    IfNull(ilHashCode, delegate () { ilHashCode.Emit(OpCodes.Ldc_I4_0); }, delegate () {
                        ilHashCode.Emit(OpCodes.Ldarg_0);
                        ilHashCode.Emit(OpCodes.Ldfld, field);
                        ilHashCode.Emit(OpCodes.Callvirt, hashMethod);
                    });
                }

                if (hasState)
                {
                    ilHashCode.Emit(OpCodes.Ldc_I4, 29);
                    ilHashCode.Emit(OpCodes.Mul);
                    ilHashCode.Emit(OpCodes.Add);
                }

                hasState = true;
                // 2.4 Add methods associated to the property.
                prop.SetGetMethod(getProperty);
                prop.SetSetMethod(setProperty);
                i++;
            }

            constructorIl.Emit(OpCodes.Ret);
            if (!hasState)
            {
                ilHashCode.Emit(OpCodes.Ldc_I4_0);
            }

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
            // dynamicAnonymousType.DefineMethodOverride(getHashCodeMethod, objHashMethod);
            return dynamicAnonymousType.CreateTypeInfo();
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

        private static void IfNull(ILGenerator il, Action isTrue, Action isFalse)
        {
            var nope = il.DefineLabel();
            var done = il.DefineLabel();
            il.Emit(OpCodes.Ldnull);
            il.Emit(OpCodes.Ceq); // Has 1 if NULL
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Ceq); // Has 1 if NOT NULL
            il.Emit(OpCodes.Brtrue, nope);
            isTrue();
            il.Emit(OpCodes.Br, done);
            il.MarkLabel(nope);
            isFalse();
            il.MarkLabel(done);
        }
    }
}
