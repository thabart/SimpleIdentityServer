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

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using SimpleIdentityServer.Configuration.Middleware;
using SimpleIdentityServer.Logging;
using SimpleIdentityServer.Oauth2Instrospection.Authentication;

namespace SimpleIdentityServer.Configuration.Host.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static void UseConfigurationService(this IApplicationBuilder app, Oauth2IntrospectionOptions options)
        {
            // Display status code page
            app.UseStatusCodePages();

            // Enable CORS
            app.UseCors("AllowAll");

            // Enable custom exception handler
            app.UseSimpleIdentityServerManagerExceptionHandler(new ExceptionHandlerMiddlewareOptions
            {
                ConfigurationEventSource = app.ApplicationServices.GetService<ConfigurationEventSource>()
            });
            
            // Enable authentication
            app.UseAuthenticationWithIntrospection(options);

            // Launch ASP.NET MVC
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action}/{id?}");
            });
        }
    }
}
