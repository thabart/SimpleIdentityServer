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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Security.Claims;
using System.Xml;

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
            /*
            Console.WriteLine("=== EID ===");
            ParseEidClaims();
            Console.WriteLine("=== Linkedin ===");
            ParseLinkedinClaims();
            Console.WriteLine("=== GitHub ===");
            ParseGitHubClaims();
            Console.WriteLine("=== Google ===");
            ParseGoogleClaims();
            Console.WriteLine("=== Facebook ===");
            ParseFacebookClaims();
            */
            Console.WriteLine("=== AUTH0 WS-FEDERATION ===");
            ParseAuth0WsFederation();
            Console.ReadLine();
        }

        #region Parsers

        static void ParseAuth0WsFederation()
        {
            var document = new XmlDocument();
            document.LoadXml(@"<saml:Assertion xmlns:saml='urn: oasis:names:tc:SAML:1.0:assertion'>
                                <saml:Conditions NotBefore='2016-08-30T08:56:51.567Z' NotOnOrAfter='2016-08-30T16:56:51.567Z'>
                                    <saml:AudienceRestrictionCondition>
                                        <saml:Audience>urn:dVJFEWakRN8w9g4H6nnZP01REE185okh</saml:Audience>
                                    </saml:AudienceRestrictionCondition>
                                </saml:Conditions>
                                <saml:AttributeStatement>
                                    <saml:Attribute AttributeName='nameidentifier'>
                                        <saml:AttributeValue>auth0|57c542a4f13883ab75f75823</saml:AttributeValue>
                                    </saml:Attribute>
                                </saml:AttributeStatement>
                            </saml:Assertion>");
            var assertionNode = document.DocumentElement;
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Auth0WsFederationParser.cs");
            var code = File.ReadAllText(filePath);
            var result = GetWsFederationClaims("Auth0WsFederationParser", code, assertionNode);
            foreach (var record in result)
            {
                Console.WriteLine(record.Type + " " + record.Value);
            }
        }

        static void ParseEidClaims()
        {
            var document = new XmlDocument();
            document.LoadXml(@"<saml:Assertion xmlns:saml='urn:oasis:names:tc:SAML:2.0:assertion'>
                <saml:Issuer>
                    http://idp.example.com/metadata.php
                </saml:Issuer>
                <saml:Subject>_ce3d2948b4cf20146dee0a0b3dd6f69b6cf86f62d7</saml:Subject>
                <saml:AttributeStatement>
                  <saml:Attribute Name='http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'>
                    <saml:AttributeValue>test@example.com</saml:AttributeValue>
                  </saml:Attribute>
                </saml:AttributeStatement>
              </saml:Assertion>");
            var assertionNode = document.DocumentElement;
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "EidClaimsParser.cs");
            var code = File.ReadAllText(filePath);
            var result = GetWsFederationClaims("EidClaimsParser", code, assertionNode);
            foreach (var record in result)
            {
                Console.WriteLine(record.Type + " " + record.Value);
            }
        }

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
            
            var result = GetOAuthClaims("LinkedinClaimsParser", code, jObj);
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

            var result = GetOAuthClaims("GitHubClaimsParser", code, jObj);
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
                        
            var result = GetOAuthClaims("GoogleClaimsParser", code, jObj);
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
            var parser = new Parser.FacebookClaimsParser();
            var result = GetOAuthClaims("FacebookClaimsParser", code, jObj);
            foreach(var record in result)
            {
                Console.WriteLine(record.Type + " " + record.Value);
            }
        }

        #endregion

        #region Helpers

        static List<Claim> GetWsFederationClaims(string className, string code, XmlNode node)
        {
            var assembly = CreateParser(className, code);
            var type = assembly.GetType("Parser." + className);
            var instance = assembly.CreateInstance("Parser." + className);
            var meth = type.GetMember("Process").First() as MethodInfo;
            var res = meth.Invoke(instance, new[] { node }) as List<Claim>;
            return res;
        }

        static List<Claim> GetOAuthClaims(string className, string code, JObject jObj)
        {
            var assembly = CreateParser(className, code);
            var type = assembly.GetType("Parser." + className);
            var instance = assembly.CreateInstance("Parser." + className);
            var meth = type.GetMember("Process").First() as MethodInfo;
            var res = meth.Invoke(instance, new[] { jObj }) as List<Claim>;
            return res;
        }

        static Assembly CreateParser(string className, string code)
        {
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
                var assembly = AssemblyLoadContext.Default.LoadFromStream(ms);
                return assembly;
            }
        }

        #endregion
    }
}
