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

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Newtonsoft.Json.Linq;
using SimpleIdentityServer.Core.Jwt;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Security.Claims;

namespace SimpleIdentityServer.ClaimsParser
{
    class Program
    {
        #region Public static methods

        public static void Main(string[] args)
        {
            ParseFacebookClaims();
            Console.ReadLine();
        }

        static void ParseFacebookClaims()
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "FacebookClaimsParser.cs");
            var code = File.ReadAllText(filePath);
            var jObj = JObject.Parse(@"{
                'id':'thabart', 
                'name':'name', 
                'first_name':'first_name', 
                'last_name':'last_name',
                'gender':'gender'
            }");
            var claims = jObj.ToObject<Dictionary<string, object>>();
            CreateFacebookClaimsParser("FacebookClaimsParser", code, claims);
        }

        static List<Claim> CreateFacebookClaimsParser(string className, string code, Dictionary<string, object> claims)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var assemblyName = Path.GetRandomFileName();
            var references = new List<MetadataReference>
            {
                MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Enumerable).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(typeof(JwsPayload).GetTypeInfo().Assembly.Location)
                // MetadataReference.CreateFromFile(typeof(BinaryReader).GetTypeInfo().Assembly.Location),
            };
            
            var location = GetAssemblyLocation("System.Runtime");
            if (!string.IsNullOrWhiteSpace(location))
            {
                references.Add(MetadataReference.CreateFromFile(location));
            }

            var compilation = CSharpCompilation.Create(
                assemblyName,
                syntaxTrees: new[] { syntaxTree },
                references: references,
                options: new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary,
                    assemblyIdentityComparer: DesktopAssemblyIdentityComparer.Default));
            using (var ms = new MemoryStream())
            {
                EmitResult result = compilation.Emit(ms);
                if (!result.Success)
                {
                    return null;
                }

                ms.Seek(0, SeekOrigin.Begin);
                var assembly = AssemblyLoadContext.Default.LoadFromStream(ms);
                var type = assembly.GetType("Parser."+ className);
                var instance = assembly.CreateInstance("Parser."+ className);
                var meth = type.GetMember("Process").First() as MethodInfo;
                var res = meth.Invoke(instance, new[] { claims });
                return null;
            }

            return null;
        }

        private static string GetAssemblyLocation(string name)
        {
            var referenced = Assembly
                .GetEntryAssembly()
                .GetReferencedAssemblies();
            var assemblyName = Assembly
                .GetEntryAssembly()
                .GetReferencedAssemblies()
                .FirstOrDefault(r => r.Name == name);
            if (assemblyName == null)
            {
                return null;
            }

            return Assembly.Load(assemblyName).Location;
        }

        #endregion
    }
}
