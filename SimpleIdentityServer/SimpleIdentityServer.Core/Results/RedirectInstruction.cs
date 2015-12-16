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

using System.Collections.Generic;
using SimpleIdentityServer.Core.Parameters;

namespace SimpleIdentityServer.Core.Results
{
    public class Parameter
    {
        public string Name { get; set; }

        public string Value { get; set; }
    }

    public class RedirectInstruction
    {
        public RedirectInstruction()
        {
            Parameters = new List<Parameter>();
        }

        public IList<Parameter> Parameters { get; private set; }

        public IdentityServerEndPoints Action { get; set; }

        public ResponseMode ResponseMode { get; set; }

        public void AddParameter(string name, string value)
        {
            var record = new Parameter
            {
                Name = name,
                Value = value
            };

            Parameters.Add(record);
        }
    }
}
