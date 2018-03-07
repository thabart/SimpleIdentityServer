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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System;
using System.Collections;
using System.Xml;
#if CORE
using System.Runtime.Loader;
#endif
using System.Security.Claims;

namespace SimpleIdentityServer.Authentication.Middleware.Parsers
{
    public interface IClaimsParser
    {
        List<Claim> Parse(
            string nameSpace,
            string className,
            string code,
            JObject jObj);

        List<Claim> Parse(
            string nameSpace,
            string className,
            string code,
            XmlNode node);
    }

    public class ClaimsParser : IClaimsParser
    {
        private static readonly List<string> _arrNames = new List<string>
        {
            SimpleIdentityServer.Core.Jwt.Constants.StandardResourceOwnerClaimNames.Role
        };

        public List<Claim> Parse(
            string nameSpace,
            string className,
            string code,
            JObject jObj)
        {
            var assembly = CreateParser(className, code);
            var type = assembly.GetType("Parser." + className);
            var instance = assembly.CreateInstance("Parser." + className);
            var meth = type.GetMember("Process").First() as MethodInfo;
            var res = meth.Invoke(instance, new[] { jObj }) as List<Claim>;
            return res;
        }

        public List<Claim> Parse(string nameSpace, string className, string code, XmlNode node)
        {
            var assembly = CreateParser(className, code);
            var type = assembly.GetType("Parser." + className);
            var instance = assembly.CreateInstance("Parser." + className);
            var meth = type.GetMember("Process").First() as MethodInfo;
            var res = meth.Invoke(instance, new[] { node }) as List<Claim>;
            return res;
        }

        private static Assembly CreateParser(string className, string code)
        {
            code = code.Replace(@"\n", "")
                .Replace(@"\t", "")
                .Replace(@"\r", "")
                .Replace(@"\f", "")
                .Replace(@"\v", "")
                .Replace(@"\", "");

            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var assemblyName = Path.GetRandomFileName();
            var references = new List<MetadataReference>
            {
                MetadataReference.CreateFromFile(typeof(Object).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Enumerable).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(typeof(JwsPayload).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(typeof(XmlNode).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(typeof(IEnumerable).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(typeof(ClaimTypes).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(typeof(JObject).GetTypeInfo().Assembly.Location)
            };

            var assemblyPath = Path.GetDirectoryName(typeof(object).GetTypeInfo().Assembly.Location);
            references.Add(MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "mscorlib.dll")));
            references.Add(MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Runtime.dll")));
            references.Add(MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.IO.dll")));

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
                Assembly assembly = null;
#if NET
                assembly = Assembly.Load(ms.ToArray());
#else
                assembly = AssemblyLoadContext.Default.LoadFromStream(ms);
#endif
                return assembly;
            }
        }
    }
}
