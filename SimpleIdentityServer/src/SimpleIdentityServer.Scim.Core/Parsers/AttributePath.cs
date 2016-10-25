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
using SimpleIdentityServer.Scim.Core.Models;
using System.Collections.Generic;
using System.Linq;

namespace SimpleIdentityServer.Scim.Core.Parsers
{
    public class AttributePath
    {
        public string Name { get; set; }
        public AttributePath Next { get; set; }
        public Filter ValueFilter { get; set; }

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
                    if (c == null || !c.Values.Any(val => val.SchemaAttribute.Name == Name))
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
                (Next != null && representations.Any(r => r.SchemaAttribute.Type != Constants.SchemaAttributeTypes.Complex)))
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
                    if (complexAttr != null && complexAttr.Values != null)
                    {
                        if (ValueFilter != null)
                        {
                            record = new ComplexRepresentationAttribute(complexAttr.SchemaAttribute)
                            {
                                Values = ValueFilter.Evaluate(complexAttr.Values)
                            };
                        }

                        if (Next != null && complexAttr.Values != null && complexAttr.Values.Any())
                        {
                            subAttrs.AddRange(Next.Evaluate(((ComplexRepresentationAttribute)record).Values));
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
