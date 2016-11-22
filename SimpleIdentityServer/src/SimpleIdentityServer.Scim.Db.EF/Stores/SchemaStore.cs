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

using System;
using System.Collections.Generic;
using SimpleIdentityServer.Scim.Core.Models;
using SimpleIdentityServer.Scim.Core.Stores;
using System.Linq;
using SimpleIdentityServer.Scim.Db.EF.Extensions;
using Microsoft.EntityFrameworkCore;

namespace SimpleIdentityServer.Scim.Db.EF.Stores
{
    internal class SchemaStore : ISchemaStore
    {
        private readonly ScimDbContext _context;
        private bool _dispose = false;

        public SchemaStore(ScimDbContext context)
        {
            _context = context;
        }

        public IEnumerable<SchemaAttributeResponse> GetCommonAttributes()
        {
            try
            {
               return  _context.SchemaAttributes.Where(s => s.IsCommon == true).Select(a => a.ToDomain());
            }
            catch
            {
                return new SchemaAttributeResponse[0];
            }
        }

        public SchemaResponse GetSchema(string id)
        {
            try
            {
                var schema = _context.Schemas.Include(s => s.Meta).Include(s => s.Attributes).FirstOrDefault(s => s.Id == id);
                return GetSchemaResponse(schema);
            }
            catch
            {
                return null;
            }
        }

        public IEnumerable<SchemaResponse> GetSchemas()
        {
            try
            {
                var schemas = _context.Schemas.Include(s => s.Meta).Include(s => s.Attributes).ToList();
                var result = new List<SchemaResponse>();
                foreach(var schema in schemas)
                {
                    var record = GetSchemaResponse(schema);
                    if (record == null)
                    {
                        continue;
                    }

                    result.Add(record);
                }

                return result;
            }
            catch
            {
                return new SchemaResponse[0];
            }
        }

        private SchemaResponse GetSchemaResponse(Models.Schema schema)
        {
            if (schema == null)
            {
                return null;
            }

            var result = schema.ToDomain();
            if (schema.Meta != null)
            {
                result.Meta = schema.Meta.ToDomain();
            }

            result.Attributes = GetAttributes(schema);
            return result;
        }

        private IEnumerable<SchemaAttributeResponse> GetAttributes(Models.Schema schema)
        {
            if (schema.Attributes == null)
            {
                return new SchemaAttributeResponse[0];
            }

            var result = new List<SchemaAttributeResponse>();
            foreach(var attr in schema.Attributes)
            {
                var transformed = TransformAttribute(attr);
                if (transformed == null)
                {
                    continue;
                }

                result.Add(transformed);
            }

            return result;
        }

        private SchemaAttributeResponse TransformAttribute(Models.SchemaAttribute attr)
        {
            var record = _context.SchemaAttributes.Include(s => s.Children).FirstOrDefault(s => s.Id == attr.Id);
            if (record == null)
            {
                return null;
            }
            
            if (record.Children != null && record.Children.Any())
            {
                var comlexSchemaAttr = new ComplexSchemaAttributeResponse();
                comlexSchemaAttr.SetData(record);
                var subAttrs = new List<SchemaAttributeResponse>();
                foreach (var child in record.Children)
                {
                    var transformed = TransformAttribute(child);
                    if (transformed == null)
                    {
                        continue;
                    }

                    subAttrs.Add(transformed);
                }

                comlexSchemaAttr.SubAttributes = subAttrs;
                return comlexSchemaAttr;
            }

            return record.ToDomain();
        }

        public void Dispose()
        {
            if (!_dispose)
            {
                _dispose = true;
                _context.Dispose();
            }
        }
    }
}
