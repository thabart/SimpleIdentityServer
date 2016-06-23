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

using EnvDTE;
using System.Collections.Generic;

namespace SimpleIdentityServer.Vse.Helpers
{
    public static class VisualStudioHelper
    {
        #region Public methods

        public static List<CodeElement> GetAllCodeElementsOfType
            (CodeElements elements,
            vsCMElement elementType, 
            bool includeExternalTypes)
        {
            var ret = new List<CodeElement>();
            foreach (CodeElement elem in elements)
            {
                if (elem.Kind == EnvDTE.vsCMElement.vsCMElementNamespace)
                {
                    ret.AddRange(GetAllCodeElementsOfType(((EnvDTE.CodeNamespace)elem).Members, elementType, includeExternalTypes));
                }
                else if (elem.InfoLocation == EnvDTE.vsCMInfoLocation.vsCMInfoLocationExternal
                        && !includeExternalTypes)
                {
                    continue;
                }
                else if (elem.IsCodeType)
                {
                    ret.AddRange(GetAllCodeElementsOfType(((EnvDTE.CodeType)elem).Members, elementType, includeExternalTypes));
                }
                if (elem.Kind == elementType)
                {
                    ret.Add(elem);
                }
            }

            return ret;
        }

        #endregion
    }
}
