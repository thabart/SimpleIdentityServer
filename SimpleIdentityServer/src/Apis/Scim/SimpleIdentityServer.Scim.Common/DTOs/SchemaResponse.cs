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
using System.Linq;
using System.Runtime.Serialization;

namespace SimpleIdentityServer.Scim.Common.DTOs
{
    [DataContract]
    public class SchemaAttributeResponse
    {
        public string FullPath
        {
            get
            {
                return GetFullPath();
            }
        }

        private string GetFullPath()
        {
            var parents = new List<SchemaAttributeResponse>();
            var names = new List<string>
            {
                Name
            };

            GetParents(this, parents);
            parents.Reverse();
            var parentNames = names.Concat(parents.Select(p => p.Name));
            return string.Join(".", parentNames);
        }

        private IEnumerable<SchemaAttributeResponse> GetParents(SchemaAttributeResponse representation, IEnumerable<SchemaAttributeResponse> parents)
        {
            if (representation.Parent == null)
            {
                return parents;
            }

            parents = parents.Concat(new[] { representation });
            return GetParents(representation.Parent, parents);
        }
        
        public SchemaAttributeResponse Parent { get; set; }

        /// <summary>
        /// Gets or sets the id
        /// </summary>
        [DataMember(Name = Constants.SchemaAttributeResponseNames.Id)]
        public string Id { get; set; }
        /// <summary>
        /// Attribute's name
        /// </summary>
        [DataMember(Name = Constants.SchemaAttributeResponseNames.Name)]
        public string Name { get; set; }
        /// <summary>
        /// Attribute's data type. Valid values are : "string", "boolean", "decimal", "integer" etc ...
        /// </summary>
        [DataMember(Name = Constants.SchemaAttributeResponseNames.Type)]
        public string Type { get; set; }
        /// <summary>
        /// Indicate attribute's plurality
        /// </summary>
        [DataMember(Name = Constants.SchemaAttributeResponseNames.MultiValued)]
        public bool MultiValued { get; set; }
        /// <summary>
        /// Human-readable description
        /// </summary>
        [DataMember(Name = Constants.SchemaAttributeResponseNames.Description)]
        public string Description { get; set; }
        /// <summary>
        /// Specifies whether or not the attribute is required
        /// </summary>
        [DataMember(Name = Constants.SchemaAttributeResponseNames.Required)]
        public bool Required { get; set; }
        /// <summary>
        /// Collection of suggested canonical values
        /// </summary>
        [DataMember(Name = Constants.SchemaAttributeResponseNames.CanonicalValues)]
        public IEnumerable<string> CanonicalValues { get; set; }
        /// <summary>
        /// Specifies whether or not a string attribute is case sensitive
        /// </summary>
        [DataMember(Name = Constants.SchemaAttributeResponseNames.CaseExact)]
        public bool CaseExact { get; set; }
        /// <summary>
        /// Circumstances under which the value of the attribute can be (re)defined
        /// </summary>
        [DataMember(Name = Constants.SchemaAttributeResponseNames.Mutability)]
        public string Mutability { get; set; }
        /// <summary>
        /// When an attribute and associated values are returned in response to a GET or in response to PUT etc ...
        /// </summary>
        [DataMember(Name = Constants.SchemaAttributeResponseNames.Returned)]
        public string Returned { get; set; }
        /// <summary>
        /// How the service provider enforces uniqueness of attribute values
        /// </summary>
        [DataMember(Name = Constants.SchemaAttributeResponseNames.Uniqueness)]
        public string Uniqueness { get; set; }
        /// <summary>
        /// Indicate the SCIM resource types that may be referenced.
        /// </summary>
        [DataMember(Name = Constants.SchemaAttributeResponseNames.ReferenceTypes)]
        public IEnumerable<string> ReferenceTypes { get; set; }
    }

    [DataContract]
    public class ComplexSchemaAttributeResponse : SchemaAttributeResponse
    {
        public ComplexSchemaAttributeResponse()
        {
            Type = Constants.SchemaAttributeTypes.Complex;
        }

        /// <summary>
        /// Defines a set of sub-attributes
        /// </summary>
        [DataMember(Name = Constants.ComplexSchemaAttributeResponseNames.SubAttributes)]
        public IEnumerable<SchemaAttributeResponse> SubAttributes { get; set; }
    }

    [DataContract]
    public class SchemaResponse : ScimResource
    {
        /// <summary>
        /// Unique URI of the schema
        /// </summary>
        [DataMember(Name = Constants.SchemaResponseNames.Id)]
        public string Id { get; set; }
        /// <summary>
        /// Human-readable name
        /// </summary>
        [DataMember(Name = Constants.SchemaResponseNames.Name)]
        public string Name { get; set; }
        /// <summary>
        /// Human-readable description
        /// </summary>
        [DataMember(Name = Constants.SchemaResponseNames.Description)]
        public string Description { get; set; }
        /// <summary>
        /// Service provider attributes and their qualities
        /// </summary>
        [DataMember(Name = Constants.SchemaResponseNames.Attributes)]
        public IEnumerable<SchemaAttributeResponse> Attributes { get; set; }
    }
}
