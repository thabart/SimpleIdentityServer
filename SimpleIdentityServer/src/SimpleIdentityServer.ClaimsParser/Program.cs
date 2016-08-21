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
        private static readonly List<string> _arrNames = new List<string>
        {
            Constants.StandardResourceOwnerClaimNames.Role
        };
        
        public static void Main(string[] args)
        {
            ParseLinkedinClaims();
            // ParseGitHubClaims();
            // ParseGoogleClaims();
            // ParseFacebookClaims();
            Console.ReadLine();
        }

        #region Parsers

        static void ParseLinkedinClaims()
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "LinkedinClaimsParser.cs");
            var code = File.ReadAllText(filePath);
            var jObj = JObject.Parse(@"{
              'firstName': 'Thierry',
              'headline': '.NET Developer',
              'id': 'EELymzTB4O',
              'lastName': 'Habart',
              'siteStandardProfileRequest': {
                'url': 'https://www.linkedin.com/profile/view?id=AAoAAAm7LEEBScvxCiD011irayzh8Rhys9NBf2Q&authType=name&authToken=0UeJ&trk=api*a4561433*s4626863*'
              }
            }");

            var claims = GetChildrenByReflection(jObj);
            var result = CreateClaimsParser("LinkedinClaimsParser", code, claims);
            foreach (var record in result)
            {
                Console.WriteLine(record.Type + " " + record.Value);
            }
        }

        static void ParseGitHubClaims()
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "GitHubClaimsParser.cs");
            var code = File.ReadAllText(filePath);
            var jObj = JObject.Parse(@"{
                'id':'thabart', 
                'name': 'name',
                'avatar_url': 'avatar_url',
                'updated_at': 'updated_at',
                'email': 'email'
            }");

            var claims = jObj.ToObject<Dictionary<string, object>>();
            var result = CreateClaimsParser("GitHubClaimsParser", code, claims);
            foreach (var record in result)
            {
                Console.WriteLine(record.Type + " " + record.Value);
            }
        }

        static void ParseGoogleClaims()
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "GoogleClaimsParser.cs");
            var code = File.ReadAllText(filePath);
            var jObj = JObject.Parse(@"{
                'id':'thabart', 
                'displayName': 'displayName',
                'name': {
                    'givenName': 'givenName',
                    'familyName': 'familyName'
                },
                'url': 'url',
                'emails': {
                    'value': 'email',
                    'type': 'personnal'
                }
            }");

            var claims = GetChildrenByReflection(jObj);
            
            var result = CreateClaimsParser("GoogleClaimsParser", code, claims);
            foreach (var record in result)
            {
                Console.WriteLine(record.Type + " " + record.Value);
            }
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
                'gender':'gender',
                'email':'email',
                'specific':'specific',
                'role':['firstrole', 'secondrole']
            }");
            var claims = jObj.ToObject<Dictionary<string, object>>();
            var parser = new Parser.FacebookClaimsParser();
            var result = CreateClaimsParser("FacebookClaimsParser", code, claims);
            foreach(var record in result)
            {
                Console.WriteLine(record.Type + " " + record.Value);
            }
        }

        #endregion

        #region Helpers

        static Dictionary<string, object> GetChildrenByReflection(JObject jArr)
        {
            var result = new Dictionary<string, object>();
            foreach (KeyValuePair<string, JToken> kvp in jArr)
            {
                var obj = kvp.Value as JObject;
                if (kvp.Value is JObject)
                {
                    result.Add(kvp.Key, GetChildrenByReflection(obj));
                }
                else
                {
                    result.Add(kvp.Key, kvp.Value);
                }
            }

            return result;
        }

        static List<Claim> CreateClaimsParser(string className, string code, Dictionary<string, object> claims)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var assemblyName = Path.GetRandomFileName();
            var references = new List<MetadataReference>
            {
                MetadataReference.CreateFromFile(typeof(Object).GetTypeInfo().Assembly.Location),
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
                var assembly = AssemblyLoadContext.Default.LoadFromStream(ms);
                var type = assembly.GetType("Parser."+ className);
                var instance = assembly.CreateInstance("Parser."+ className);
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
                        foreach(var arrayRecord in GetArray(record.Value))
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
