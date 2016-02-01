using Microsoft.AspNet.Builder;
using System;

namespace SimpleIdentityServer.Host.MiddleWare
{
    public static class SimpleIdentityServerExceptionExtensions
    {
        public static IApplicationBuilder UseSimpleIdentityServerExceptionHandler(
            this IApplicationBuilder applicationBuilder,
            ExceptionHandlerMiddlewareOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            return applicationBuilder.UseMiddleware<ExceptionHandlerMiddleware>(options);
        }
    }
}
