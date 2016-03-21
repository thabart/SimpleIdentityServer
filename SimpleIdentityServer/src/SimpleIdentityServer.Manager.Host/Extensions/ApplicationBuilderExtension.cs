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

using Microsoft.AspNet.Builder;
using SimpleIdentityServer.Manager.Host.Middleware;
using SimpleIdentityServer.UserInformation.Authentication;
using System;

namespace SimpleIdentityServer.Manager.Host.Extensions
{
    public static class ApplicationBuilderExtension
    {
        public static void UseSimpleIdentityServerManager(
            this IApplicationBuilder applicationBuilder,
            AuthorizationServerOptions authorizationServerOptions,
            SwaggerOptions swaggerOptions)
        {
            if (authorizationServerOptions == null)
            {
                throw new ArgumentNullException(nameof(authorizationServerOptions));
            }

            if (swaggerOptions == null)
            {
                throw new ArgumentNullException(nameof(swaggerOptions));
            }

            // Display status code page
            applicationBuilder.UseStatusCodePages();

            // Enable CORS
            applicationBuilder.UseCors("AllowAll");

            // Enable custom exception handler
            applicationBuilder.UseSimpleIdentityServerManagerExceptionHandler();
            var userInformationOptions = new UserInformationOptions
            {
                UserInformationEndPoint = authorizationServerOptions.UserInformationUrl
            };
            applicationBuilder.UseAuthenticationWithUserInformation(userInformationOptions);

            // Launch ASP.NET MVC
            applicationBuilder.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action}/{id?}");
            });

            // Launch swagger
            if (swaggerOptions.IsEnabled)
            {
                applicationBuilder.UseSwaggerGen();
                applicationBuilder.UseSwaggerUi();
            }
        }
    }
}
