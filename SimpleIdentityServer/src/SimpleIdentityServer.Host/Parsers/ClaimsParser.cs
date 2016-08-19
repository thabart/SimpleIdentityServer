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
#if CORE
using System.Runtime.Loader;
#endif
using System.Security.Claims;

namespace SimpleIdentityServer.Host.Parsers
{
    public interface IClaimsParser
    {
        List<Claim> Parse(
            string nameSpace,
            string className,
            string code,
            Dictionary<string, object> claims);
    }

    public class ClaimsParser : IClaimsParser
    {
        #region Fields

        private static readonly List<string> _arrNames = new List<string>
        {
            Core.Jwt.Constants.StandardResourceOwnerClaimNames.Role
        };

        #endregion

        #region Public methods

        public List<Claim> Parse(
            string nameSpace, 
            string className, 
            string code, 
            Dictionary<string, object> claims)
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
                MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Enumerable).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(typeof(JwsPayload).GetTypeInfo().Assembly.Location)
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
                Assembly assembly = null;
#if NET
                assembly = Assembly.Load(ms.ToArray());
#else
                assembly = AssemblyLoadContext.Default.LoadFromStream(ms);
#endif
                var type = assembly.GetType(nameSpace+"."+ className);
                var instance = assembly.CreateInstance(nameSpace + "." + className);
                var meth = type.GetMember("Process").First() as MethodInfo;
                var res = meth.Invoke(instance, new[] { claims }) as Dictionary<string, object>;
                if (res == null)
                {
                    return null;
                }

                var cls = new List<Claim>();
                foreach (var record in res)
                {
                    if (_arrNames.Contains(record.Key))
                    {
                        foreach (var arrayRecord in GetArray(record.Value))
                        {
                            cls.Add(new Claim(record.Key, arrayRecord));
                        }

                        continue;
                    }

                    cls.Add(new Claim(record.Key, record.Value.ToString()));
                }

                return cls;
            }
        }

        #endregion

        #region Private methods

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

        private static string[] GetArray(object obj)
        {
            var arr = obj as object[];
            var jArr = obj as JArray;
            if (arr != null)
            {
                return arr.Select(c => c.ToString()).ToArray();
            }

            if (jArr != null)
            {
                return jArr.Select(c => c.ToString()).ToArray();
            }

            return new string[0];
        }

        #endregion
    }
}
