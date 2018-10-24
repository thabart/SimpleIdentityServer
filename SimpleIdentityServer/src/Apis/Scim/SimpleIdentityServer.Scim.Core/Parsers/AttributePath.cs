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

using Newtonsoft.Json.Linq;
using SimpleIdentityServer.Scim.Common.DTOs;
using SimpleIdentityServer.Scim.Common.Models;
using System.Collections.Generic;
using System.Linq;

namespace SimpleIdentityServer.Scim.Core.Parsers
{
    public class AttributePath
    {
        public string Name { get; set; }
        public AttributePath Next { get; set; }
        public AttributePath Parent { get; set; }
        public Filter ValueFilter { get; set; }

        public KeyValuePair<AttributePath, int> GetLastPath(int i = 0)
        {
            if (Next != null)
            {
                return Next.GetLastPath(++i);
            }

            return new KeyValuePair<AttributePath, int>(this, i);
        }

        public void SetNext(AttributePath next)
        {
            if (next == null)
            {
                Next = null;
                return;
            }

            Next = next;
            Next.Parent = this;
        }

        public string Evaluate(JToken jObj)
        {
            var token = jObj.SelectToken(Name);
            if (token == null)
            {
                return null;
            }

            if (Next != null)
            {
                return Next.Evaluate(token);
            }

            return token.ToString();
        }

        public IEnumerable<RepresentationAttribute> Evaluate(IEnumerable<RepresentationAttribute> representationAttrs)
        {
            var representations = representationAttrs.Where(r => r.SchemaAttribute != null && r.SchemaAttribute.Name == Name);
            if (!representations.Any())
            {
                var lst = new List<RepresentationAttribute>();
                foreach (var representationAttr in representationAttrs)
                {
                    var c = representationAttr as ComplexRepresentationAttribute;
                    if (c == null || !c.Values.Any(val => val.SchemaAttribute != null && val.SchemaAttribute.Name == Name))
                    {
                        continue;
                    }

                    lst.AddRange(c.Values.Where(val => val.SchemaAttribute.Name == Name));
                }

                if (!lst.Any())
                {
                    return new RepresentationAttribute[0];
                }

                representations = lst;
            }

            // representations = representations.Select(r => (RepresentationAttribute)r.Clone()).ToList();
            if ((ValueFilter != null && representations.Any(r => !r.SchemaAttribute.MultiValued)) ||
                (Next != null && representations.Any(r => r.SchemaAttribute.Type != Common.Constants.SchemaAttributeTypes.Complex)))
            {
                return null;
            }

            if (ValueFilter != null || Next != null)
            {
                var subAttrs = new List<RepresentationAttribute>();
                var lst = new List<RepresentationAttribute>();
                foreach (var representation in representations)
                {
                    var record = representation;
                    var complexAttr = representation as ComplexRepresentationAttribute;
                    if (complexAttr != null && complexAttr.Values != null && complexAttr.Values.Any())
                    {
                        if (ValueFilter != null)
                        {
                            var val = ValueFilter.Evaluate(complexAttr.Values);
                            if (val != null && val.Any())
                            {
                                record = new ComplexRepresentationAttribute(complexAttr.SchemaAttribute)
                                {
                                    Values = val
                                };
                                lst.Add(record);
                            }
                        }

                        if (Next != null)
                        {
                            var values = Next.Evaluate(((ComplexRepresentationAttribute)record).Values);
                            if (values != null && values.Any())
                            {
                                subAttrs.AddRange(values);
                            }
                        }
                    }
                }

                representations = lst;
                if (subAttrs.Any())
                {
                    return subAttrs;
                }
            }

            return representations;
        }

        public IEnumerable<SchemaAttributeResponse> Evaluate(IEnumerable<SchemaAttributeResponse> schemaAttrs)
        {
            var representations = schemaAttrs.Where(r => r.Name == Name);
            if (!representations.Any())
            {
                var lst = new List<SchemaAttributeResponse>();
                foreach (var schemaAttr in schemaAttrs)
                {
                    var c = schemaAttr as ComplexSchemaAttributeResponse;
                    if (c == null || !c.SubAttributes.Any(val => val.Name == Name))
                    {
                        continue;
                    }

                    lst.AddRange(c.SubAttributes.Where(val => val.Name == Name));
                }

                if (!lst.Any())
                {
                    return new SchemaAttributeResponse[0];
                }

                representations = lst;
            }
            
            if ((ValueFilter != null && representations.Any(r => !r.MultiValued)) ||
                (Next != null && representations.Any(r => r.Type != Common.Constants.SchemaAttributeTypes.Complex)))
            {
                return null;
            }

            if (ValueFilter != null || Next != null)
            {
                var subAttrs = new List<SchemaAttributeResponse>();
                var lst = new List<SchemaAttributeResponse>();
                foreach (var representation in representations)
                {
                    var record = representation;
                    var complexAttr = representation as ComplexSchemaAttributeResponse;
                    if (complexAttr != null && complexAttr.SubAttributes != null)
                    {
                        if (ValueFilter != null)
                        {
                            record = new ComplexSchemaAttributeResponse()
                            {
                                SubAttributes = ValueFilter.Evaluate(complexAttr.SubAttributes)
                            };
                        }

                        if (Next != null && complexAttr.SubAttributes != null && complexAttr.SubAttributes.Any())
                        {
                            subAttrs.AddRange(Next.Evaluate(((ComplexSchemaAttributeResponse)record).SubAttributes));
                        }
                    }

                    lst.Add(record);
                }

                representations = lst;
                if (subAttrs.Any())
                {
                    return subAttrs;
                }
            }

            return representations;
        }
    }
}
