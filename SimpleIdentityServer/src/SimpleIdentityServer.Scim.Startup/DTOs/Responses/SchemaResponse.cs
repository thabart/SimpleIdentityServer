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
using System.Runtime.Serialization;
using static SimpleIdentityServer.Scim.Startup.Constants;

namespace SimpleIdentityServer.Scim.Startup.DTOs.Responses
{
    [DataContract]
    public class SchemaAttributeResponse
    {
        /// <summary>
        /// Attribute's name
        /// </summary>
        [DataMember(Name = SchemaAttributeResponseNames.Name)]
        public string Name { get; set; }
        /// <summary>
        /// Attribute's data type. Valid values are : "string", "boolean", "decimal", "integer" etc ...
        /// </summary>
        [DataMember(Name = SchemaAttributeResponseNames.Type)]
        public string Type { get; set; }
        /// <summary>
        /// Indicate attribute's plurality
        /// </summary>
        [DataMember(Name = SchemaAttributeResponseNames.MultiValued)]
        public bool MultiValued { get; set; }
        /// <summary>
        /// Human-readable description
        /// </summary>
        [DataMember(Name = SchemaAttributeResponseNames.Description)]
        public string Description { get; set; }
        /// <summary>
        /// Specifies whether or not the attribute is required
        /// </summary>
        [DataMember(Name = SchemaAttributeResponseNames.Required)]
        public bool Required { get; set; }
        /// <summary>
        /// Collection of suggested canonical values
        /// </summary>
        [DataMember(Name = SchemaAttributeResponseNames.CanonicalValues)]
        public IEnumerable<string> CanonicalValues { get; set; }
        /// <summary>
        /// Specifies whether or not a string attribute is case sensitive
        /// </summary>
        [DataMember(Name = SchemaAttributeResponseNames.CaseExact)]
        public bool CaseExact { get; set; }
        /// <summary>
        /// Circumstances under which the value of the attribute can be (re)defined
        /// </summary>
        [DataMember(Name = SchemaAttributeResponseNames.Mutability)]
        public string Mutability { get; set; }
        /// <summary>
        /// When an attribute and associated values are returned in response to a GET or in response to PUT etc ...
        /// </summary>
        [DataMember(Name = SchemaAttributeResponseNames.Returned)]
        public string Returned { get; set; }
        /// <summary>
        /// How the service provider enforces uniqueness of attribute values
        /// </summary>
        [DataMember(Name = SchemaAttributeResponseNames.Uniqueness)]
        public string Uniqueness { get; set; }
        /// <summary>
        /// Indicate the SCIM resource types that may be referenced.
        /// </summary>
        [DataMember(Name = SchemaAttributeResponseNames.ReferenceTypes)]
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
        [DataMember(Name = ComplexSchemaAttributeResponseNames.SubAttributes)]
        public IEnumerable<SchemaAttributeResponse> SubAttributes { get; set; }
    }

    [DataContract]
    internal class SchemaResponse : ScimResourceResponse
    {
        /// <summary>
        /// Unique URI of the schema
        /// </summary>
        [DataMember(Name = SchemaResponseNames.Id)]
        public string Id { get; set; }
        /// <summary>
        /// Human-readable name
        /// </summary>
        [DataMember(Name = SchemaResponseNames.Name)]
        public string Name { get; set; }
        /// <summary>
        /// Human-readable description
        /// </summary>
        [DataMember(Name = SchemaResponseNames.Description)]
        public string Description { get; set; }
        /// <summary>
        /// Service provider attributes and their qualities
        /// </summary>
        [DataMember(Name = SchemaResponseNames.Attributes)]
        public IEnumerable<SchemaAttributeResponse> Attributes { get; set; }
    }
}
