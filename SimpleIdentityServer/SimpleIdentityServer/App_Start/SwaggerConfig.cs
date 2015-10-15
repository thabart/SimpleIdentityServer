using System.Web.Http;

using Swashbuckle.Application;

namespace SimpleIdentityServer.Api
{
    public class SwaggerConfig
    {
        public static void Configure(HttpConfiguration configuration)
        {
            configuration.EnableSwagger(c =>
            {
                c.SingleApiVersion("v1", "SimpleIdentityServer.Api");
                c.OperationFilter<SwaggerOperationNameFilter>();
            }).EnableSwaggerUi();
        }
    }
}
