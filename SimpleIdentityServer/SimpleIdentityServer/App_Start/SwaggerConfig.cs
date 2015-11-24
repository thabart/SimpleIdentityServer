using System;
using System.Net.Http;
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
                c.RootUrl(GetRootUrlFromAppConfig);
                c.SingleApiVersion("v1", "SimpleIdentityServer.Api");
                c.OperationFilter<SwaggerOperationNameFilter>();

            }).EnableSwaggerUi();
        }

        private static string GetRootUrlFromAppConfig(HttpRequestMessage req)
        {
            //Known issue : https://github.com/domaindrivendev/Swashbuckle#owin-hosted-in-iis---incorrect-virtualpathroot-handling
            return req.RequestUri.GetLeftPart(UriPartial.Authority) + req.GetRequestContext().VirtualPathRoot.TrimEnd('/');
        }   
    }
}
