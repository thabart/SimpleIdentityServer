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

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;

namespace SimpleIdentityServer.Uma.Core.Code
{
    public interface ICodeProvider
    {
        MemoryStream GetFiles(Languages language, TypeCode type);
    }

    public enum Languages
    {
        Csharp
    }

    public enum TypeCode
    {
        Backend,
        Frontend
    }

    internal class CodeProvider : ICodeProvider
    {
        private readonly Dictionary<TypeCode, string> _mappingTypeToCodes = new Dictionary<TypeCode, string>
        {
            {
                TypeCode.Backend,
                "Backend"
            },
            {
                TypeCode.Frontend,
                "Backend"
            }
        };

        #region Public methods

        public MemoryStream GetFiles(Languages language, TypeCode type)
        {
            var codeLanguage = Constants.MappingLanguageToCodes[language];
            var typeCode = _mappingTypeToCodes[type];
            var assembly = typeof(CodeProvider).GetTypeInfo().Assembly;
            var resourceNames = from resourceName in assembly.GetManifestResourceNames()
                                where resourceName.Contains($"SimpleIdentityServer.Uma.Core.Code.{typeCode}.{codeLanguage}")
                                select resourceName;
            var resourceNameToFileName = new Dictionary<string, string>();
            foreach (var resourceName in resourceNames)
            {
                var fileName = GetFileNameFromResourceName(resourceName);
                resourceNameToFileName.Add(resourceName, fileName);
            }

            var memoryStream = new MemoryStream();
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                foreach (var kv in resourceNameToFileName)
                {
                    var file = archive.CreateEntry(kv.Value);
                    using (var fileStream = file.Open())
                    {
                        using (var resource = assembly.GetManifestResourceStream(kv.Key))
                        {
                            resource.CopyTo(fileStream);
                        }
                    }
                }
            }

            return memoryStream;
        }

        #endregion

        #region Private static methods

        private static string GetFileNameFromResourceName(string resourceName)
        {
            // A.B.C.D.filename.extension
            var parts = resourceName.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2)
            {
                return null;
            }

            // filename.extension.template
            if (parts.Length > 2 && string.Equals("template", parts[parts.Length - 1], StringComparison.OrdinalIgnoreCase))
            {
                return parts[parts.Length - 3] + "." + parts[parts.Length - 2];
            }

            // filename.extension
            return parts[parts.Length - 2] + "." + parts[parts.Length - 1];
        }

        #endregion
    }
}
