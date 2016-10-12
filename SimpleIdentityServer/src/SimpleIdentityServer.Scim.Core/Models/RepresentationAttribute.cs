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

namespace SimpleIdentityServer.Scim.Core.Models
{
    public class RepresentationAttribute
    {
        public RepresentationAttribute(string type)
        {
            Type = type;
        }

        public string Type { get; private set; }
    }

    public class SingularRepresentationAttribute<T> : RepresentationAttribute
    {
        public SingularRepresentationAttribute(string type, T value): base(type)
        {
            Value = value;
        }

        public T Value { get; private set; }
    }

    public class ComplexRepresentationAttribute : RepresentationAttribute
    {
        public ComplexRepresentationAttribute(string type): base(type)
        {
        }

        public IEnumerable<RepresentationAttribute> Values { get; set; }
    }
}
