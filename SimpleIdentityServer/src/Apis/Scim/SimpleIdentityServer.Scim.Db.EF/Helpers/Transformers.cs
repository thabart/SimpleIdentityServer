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

using Newtonsoft.Json;
using SimpleIdentityServer.Scim.Common.DTOs;
using SimpleIdentityServer.Scim.Core.Models;
using SimpleIdentityServer.Scim.Db.EF.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using Model = SimpleIdentityServer.Scim.Db.EF.Models;

namespace SimpleIdentityServer.Scim.Db.EF.Helpers
{
    public interface ITransformers
    {
        SchemaAttributeResponse Transform(Models.SchemaAttribute attr);
        RepresentationAttribute Transform(Model.RepresentationAttribute attr);
        Model.RepresentationAttribute Transform(RepresentationAttribute attr);
    }

    public class Transformers : ITransformers
    {
        public SchemaAttributeResponse Transform(Models.SchemaAttribute record)
        {
            if (record == null)
            {
                return null;
            }

            if (record.Type == Common.Constants.SchemaAttributeTypes.Complex)
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
            if (attr == null || (attr.Children == null && string.IsNullOrWhiteSpace(attr.Value)) && attr.ValueNumber == default(double))
            {
                return null;
            }

            SchemaAttributeResponse schemaAttr = null;
            if (attr.SchemaAttribute != null)
            {
                schemaAttr = Transform(attr.SchemaAttribute);
            }

            if (attr.SchemaAttribute != null && attr.SchemaAttribute.Type == Common.Constants.SchemaAttributeTypes.Complex ||
                attr.SchemaAttribute == null && attr.Children != null && attr.Children.Any())
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
                case Common.Constants.SchemaAttributeTypes.String:
                    if (isArr)
                    {
                        var values = new List<string>();
                        if (attr.Values != null)
                        {
                            foreach (var value in attr.Values)
                            {
                                values.Add(value.Value);
                            }
                        }
                        return new SingularRepresentationAttribute<IEnumerable<string>>(schemaAttr, values);
                    }                    
                    return new SingularRepresentationAttribute<string>(schemaAttr, attr.Value);
                case Common.Constants.SchemaAttributeTypes.Boolean:
                    bool r = false;
                    if (isArr)
                    {
                        var values = new List<bool>();
                        if (attr.Values != null)
                        {
                            foreach (var value in attr.Values)
                            {
                                if (bool.TryParse(value.Value, out r))
                                {
                                    values.Add(r);
                                }
                            }
                        }
                        return new SingularRepresentationAttribute<IEnumerable<bool>>(schemaAttr, values);
                    }
                    bool.TryParse(attr.Value, out r);
                    return new SingularRepresentationAttribute<bool>(schemaAttr, r);
                case Common.Constants.SchemaAttributeTypes.DateTime:
                    DateTime dateTime = DateTime.Now;
                    double d;
                    if (isArr)
                    {
                        var values = new List<DateTime>();
                        if (attr.Values != null)
                        {
                            foreach (var value in attr.Values)
                            {
                                if (double.TryParse(value.Value, out d))
                                {
                                    values.Add(d.ToDateTime());
                                }
                            }
                        }
                        return new SingularRepresentationAttribute<IEnumerable<DateTime>>(schemaAttr, values);
                    }
                    if (attr.ValueNumber != default(double))
                    {
                        dateTime = attr.ValueNumber.ToDateTime();
                    }
                    return new SingularRepresentationAttribute<DateTime>(schemaAttr, dateTime);
                case Common.Constants.SchemaAttributeTypes.Decimal:
                    decimal dec;
                    if (isArr)
                    {
                        var values = new List<decimal>();
                        if (attr.Values != null)
                        {
                            foreach (var value in attr.Values)
                            {
                                if (decimal.TryParse(value.Value, out dec))
                                {
                                    values.Add(dec);
                                }
                            }
                        }
                        return new SingularRepresentationAttribute<IEnumerable<decimal>>(schemaAttr, values);
                    }
                    return new SingularRepresentationAttribute<decimal>(schemaAttr, (decimal)attr.ValueNumber);
                case Common.Constants.SchemaAttributeTypes.Integer:
                    int i;
                    if (isArr)
                    {
                        var values = new List<int>();
                        if (attr.Values != null)
                        {
                            foreach (var value in attr.Values)
                            {
                                if (int.TryParse(value.Value, out i))
                                {
                                    values.Add(i);
                                }
                            }
                        }
                        return new SingularRepresentationAttribute<IEnumerable<int>>(schemaAttr, values);
                    }
                    return new SingularRepresentationAttribute<int>(schemaAttr, (int)attr.ValueNumber);
            }

            return null;
        }

        public Model.RepresentationAttribute Transform(RepresentationAttribute attr)
        {
            var record = new Model.RepresentationAttribute
            {
                Id = Guid.NewGuid().ToString(),
                Children = new List<Model.RepresentationAttribute>()
            };
            if (attr.SchemaAttribute != null)
            {
                record.SchemaAttributeId = attr.SchemaAttribute.Id;
            }

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

            
            if (attr.SchemaAttribute.MultiValued)
            {
                var singular = attr as SingularRepresentationAttribute<IEnumerable<string>>;
                if (singular != null)
                {
                    var representationAttributeValues = new List<Model.RepresentationAttributeValue>();
                    switch (attr.SchemaAttribute.Type)
                    {
                        case Common.Constants.SchemaAttributeTypes.Boolean:
                        case Common.Constants.SchemaAttributeTypes.String:
                        case Common.Constants.SchemaAttributeTypes.Integer:
                        case Common.Constants.SchemaAttributeTypes.Decimal:
                            foreach (var value in singular.Value)
                            {
                                representationAttributeValues.Add(new Model.RepresentationAttributeValue
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    Value = value.ToString()
                                });
                            }
                            break;
                        case Common.Constants.SchemaAttributeTypes.DateTime:
                            foreach (var value in singular.Value)
                            {
                                DateTime dt;
                                if (DateTime.TryParse(value, out dt))
                                {
                                    representationAttributeValues.Add(new Model.RepresentationAttributeValue
                                    {
                                        Id = Guid.NewGuid().ToString(),
                                        Value = dt.ToUnix().ToString()
                                    });
                                }
                            }
                            break;
                    }

                    record.Values = representationAttributeValues;
                }
            }
            else
            {
                var value = attr.GetValue();                
                switch (attr.SchemaAttribute.Type)
                {
                    case Common.Constants.SchemaAttributeTypes.Boolean:
                    case Common.Constants.SchemaAttributeTypes.String:
                        record.Value = value.ToString();
                        break;
                    case Common.Constants.SchemaAttributeTypes.Decimal:
                        var dec = (decimal)value;
                        record.ValueNumber = (double)dec;
                        break;
                    case Common.Constants.SchemaAttributeTypes.Integer:
                        var i = (int)value;
                        record.ValueNumber = i;
                        break;
                    case Common.Constants.SchemaAttributeTypes.DateTime:
                        var d = (DateTime)value;
                        record.ValueNumber = d.ToUnix();
                        break;
                }
            }

            return record;
        }
    }
}
