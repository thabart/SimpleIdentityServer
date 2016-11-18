#region copyright
// Copyright 2016 Habart Thierry
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

namespace SimpleIdentityServer.Scim.Db.InMemory.Models
{
    public class SchemaAttribute
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public bool MultiValued { get; set; }
        public string Description { get; set; }
        public bool Required { get; set; }
        public string CanonicalValues { get; set; }
        public bool CaseExact { get; set; }
        public string Mutability { get; set; }
        public string Returned { get; set; }
        public string Uniqueness { get; set; }
        public string ReferenceTypes { get; set; }
        public string SchemaAttributeIdParent { get; set; }
        public string SchemaId { get; set; }
        public bool IsCommon { get; set; }
        public virtual SchemaAttribute Parent { get; set; }
        public virtual List<RepresentationAttribute> RepresentationAttributes { get; set; }
        public virtual List<SchemaAttribute> Children { get; set; }
        public virtual Schema Schema { get; set; }
    }
}
