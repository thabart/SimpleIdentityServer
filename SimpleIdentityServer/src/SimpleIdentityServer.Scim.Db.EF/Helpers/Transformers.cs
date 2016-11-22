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

using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SimpleIdentityServer.Scim.Core;
using SimpleIdentityServer.Scim.Core.Models;
using SimpleIdentityServer.Scim.Db.EF.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using Model = SimpleIdentityServer.Scim.Db.EF.Models;

namespace SimpleIdentityServer.Scim.Db.EF.Helpers
{
    internal interface ITransformers
    {
        SchemaAttributeResponse Transform(Models.SchemaAttribute attr);
        RepresentationAttribute Transform(Model.RepresentationAttribute attr);
        Model.RepresentationAttribute Transform(RepresentationAttribute attr);
    }

    internal class Transformers : ITransformers
    {
        public SchemaAttributeResponse Transform(Models.SchemaAttribute record)
        {
            if (record == null)
            {
                return null;
            }

            if (record.Type == Constants.SchemaAttributeTypes.Complex)
            {
                var comlexSchemaAttr = new ComplexSchemaAttributeResponse();
                comlexSchemaAttr.SetData(record);
                var subAttrs = new List<SchemaAttributeResponse>();
                if (record.Children != null)
                {
                    foreach (var child in record.Children)
                    {
                        var transformed = Transform(child);
                        if (transformed == null)
                        {
                            continue;
                        }

                        subAttrs.Add(transformed);
                    }
                }

                comlexSchemaAttr.SubAttributes = subAttrs;
                return comlexSchemaAttr;
            }

            return record.ToDomain();
        }

        public RepresentationAttribute Transform(Model.RepresentationAttribute attr)
        {
            if (string.IsNullOrWhiteSpace(attr.SchemaAttributeId))
            {
                return null;
            }
            
            if (attr == null || attr.SchemaAttribute == null || (attr.Children == null && string.IsNullOrWhiteSpace(attr.Value)))
            {
                return null;
            }
            
            var schemaAttr = Transform(attr.SchemaAttribute);
            if (attr.SchemaAttribute.Type == Constants.SchemaAttributeTypes.Complex)
            {
                ComplexRepresentationAttribute result = new ComplexRepresentationAttribute(schemaAttr);
                result.Values = new List<RepresentationAttribute>();
                foreach (var child in attr.Children)
                {
                    var transformed = Transform(child);
                    if (transformed == null)
                    {
                        continue;
                    }

                    transformed.Parent = result;
                    result.Values = result.Values.Concat(new[] { transformed });
                }

                return result;
            }

            var isArr = attr.SchemaAttribute.MultiValued;
            switch (attr.SchemaAttribute.Type)
            {
                case Constants.SchemaAttributeTypes.String:
                    if (isArr)
                    {
                        var record = JsonConvert.DeserializeObject<IEnumerable<string>>(attr.Value);
                        return new SingularRepresentationAttribute<IEnumerable<string>>(schemaAttr, record);
                    }

                    var str = JsonConvert.DeserializeObject<string>(attr.Value);
                    return new SingularRepresentationAttribute<string>(schemaAttr, str);
                case Constants.SchemaAttributeTypes.Boolean:
                    if (isArr)
                    {
                        var record = JsonConvert.DeserializeObject<IEnumerable<bool>>(attr.Value);
                        return new SingularRepresentationAttribute<IEnumerable<bool>>(schemaAttr, record);
                    }

                    var b = JsonConvert.DeserializeObject<bool>(attr.Value);
                    return new SingularRepresentationAttribute<bool>(schemaAttr, b);
                case Constants.SchemaAttributeTypes.DateTime:
                    if (isArr)
                    {
                        var record = JsonConvert.DeserializeObject<IEnumerable<DateTime>>(attr.Value);
                        return new SingularRepresentationAttribute<IEnumerable<DateTime>>(schemaAttr, record);
                    }

                    var datetime = JsonConvert.DeserializeObject<DateTime>(attr.Value);
                    return new SingularRepresentationAttribute<DateTime>(schemaAttr, datetime);
                case Constants.SchemaAttributeTypes.Decimal:
                    if (isArr)
                    {
                        var record = JsonConvert.DeserializeObject<IEnumerable<decimal>>(attr.Value);
                        return new SingularRepresentationAttribute<IEnumerable<decimal>>(schemaAttr, record);
                    }

                    var dec = JsonConvert.DeserializeObject<decimal>(attr.Value);
                    return new SingularRepresentationAttribute<decimal>(schemaAttr, dec);
                case Constants.SchemaAttributeTypes.Integer:
                    if (isArr)
                    {
                        var record = JsonConvert.DeserializeObject<IEnumerable<int>>(attr.Value);
                        return new SingularRepresentationAttribute<IEnumerable<int>>(schemaAttr, record);
                    }

                    var i = JsonConvert.DeserializeObject<int>(attr.Value);
                    return new SingularRepresentationAttribute<int>(schemaAttr, i);
            }

            return null;
        }

        public Model.RepresentationAttribute Transform(RepresentationAttribute attr)
        {
            if (attr.SchemaAttribute == null)
            {
                return null;
            }

            var record = new Model.RepresentationAttribute
            {
                Id = Guid.NewGuid().ToString(),
                SchemaAttributeId = attr.SchemaAttribute.Id,
                Children = new List<Model.RepresentationAttribute>()
            };
            var complexAttr = attr as ComplexRepresentationAttribute;
            if (complexAttr != null)
            {
                if (complexAttr.Values != null)
                {
                    foreach (var child in complexAttr.Values)
                    {
                        var transformed = Transform(child);
                        if (transformed == null)
                        {
                            continue;
                        }

                        transformed.Parent = record;
                        record.Children.Add(transformed);
                    }
                }
                return record;
            }

            record.Value = attr.GetSerializedValue();
            return record;
        }
    }
}
