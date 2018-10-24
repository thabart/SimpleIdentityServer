using SimpleIdentityServer.Scim.Common.DTOs;
using SimpleIdentityServer.Scim.Core.EF.Extensions;
using SimpleIdentityServer.Scim.Core.EF.Helpers;
using SimpleIdentityServer.Scim.Core.Stores;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Scim.Core.EF.Stores
{
    internal sealed class DefaultSchemaStore : ISchemaStore
    {
        private List<Core.EF.Models.Schema> _schemas;
        private ITransformers _transformers;

        public DefaultSchemaStore(List<Core.EF.Models.Schema> schemas)
        {
            _schemas = schemas == null ? new List<Models.Schema>
            {
                DefaultSchemas.GroupSchema,
                DefaultSchemas.UserSchema
            } : schemas;
            _transformers = new Transformers();
        }

        public Task<IEnumerable<SchemaAttributeResponse>> GetCommonAttributes()
        {
            var schemaAttributes = GetAllSchemaAttributes();
            var attrs = schemaAttributes.Where(s => s.IsCommon == true);
            var result = new List<SchemaAttributeResponse>();
            foreach (var attr in attrs)
            {
                result.Add(_transformers.Transform(attr));
            }

            return Task.FromResult((IEnumerable<SchemaAttributeResponse>)result);
        }

        public Task<SchemaResponse> GetSchema(string id)
        {
            var schema = _schemas.FirstOrDefault(s => s.Id == id);
            if (schema == null)
            {
                return Task.FromResult((SchemaResponse)null);
            }

            return Task.FromResult(GetSchemaResponse(schema));
        }

        public Task<IEnumerable<SchemaResponse>> GetSchemas()
        {
            var schemas = _schemas;
            var result = new List<SchemaResponse>();
            foreach (var schema in schemas)
            {
                var record = GetSchemaResponse(schema);
                if (record == null)
                {
                    continue;
                }

                result.Add(record);
            }

            return Task.FromResult((IEnumerable<SchemaResponse>)result);
        }

        private SchemaResponse GetSchemaResponse(Core.EF.Models.Schema schema)
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

        public void Dispose()
        {
        }

        private IEnumerable<SchemaAttributeResponse> GetAttributes(Core.EF.Models.Schema schema)
        {
            if (schema.Attributes == null)
            {
                return new SchemaAttributeResponse[0];
            }

            var result = new List<SchemaAttributeResponse>();
            foreach (var attr in schema.Attributes)
            {
                var transformed = _transformers.Transform(attr);
                if (transformed == null)
                {
                    continue;
                }

                result.Add(transformed);
            }

            return result;
        }

        private List<Core.EF.Models.SchemaAttribute> GetAllSchemaAttributes()
        {
            var schemaAttributes = new List<Core.EF.Models.SchemaAttribute>();
            foreach (var s in _schemas)
            {
                if (s.Attributes == null)
                {
                    continue;
                }

                foreach (var attr in s.Attributes)
                {
                    GetSchemaAttributes(attr, schemaAttributes);
                }
            }

            return schemaAttributes;
        }

        private void GetSchemaAttributes(Core.EF.Models.SchemaAttribute schemaAttr, List<Core.EF.Models.SchemaAttribute> schemaAttrs)
        {
            schemaAttrs.Add(schemaAttr);
            if (schemaAttr.Children != null)
            {
                foreach(var child in schemaAttr.Children)
                {
                    GetSchemaAttributes(child, schemaAttrs);
                }
            }
        }
    }
}
