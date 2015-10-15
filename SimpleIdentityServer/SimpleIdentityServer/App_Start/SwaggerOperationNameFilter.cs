using SimpleIdentityServer.Api.Attributes;
using Swashbuckle.Swagger;

using System.Linq;
using System.Web.Http.Description;

namespace SimpleIdentityServer.Api
{
    public class SwaggerOperationNameFilter : IOperationFilter
    {
        public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
        {
            operation.operationId = apiDescription.ActionDescriptor
              .GetCustomAttributes<SwaggerOperationAttribute>()
              .Select(a => a.OperationId)
              .FirstOrDefault();
        }
    }
}