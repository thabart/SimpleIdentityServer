using System;

namespace SimpleIdentityServer.Api.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class SwaggerOperationAttribute : Attribute
    {
        public SwaggerOperationAttribute(string operationId)
        {
            this.OperationId = operationId;
        }

        public string OperationId { get; private set; }
    }
}