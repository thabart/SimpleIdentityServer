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
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using SimpleIdentityServer.DataAccess.SqlServer;
using SimpleIdentityServer.DataAccess.SqlServer.Extensions;
using SimpleIdentityServer.Host.MiddleWare;
using SimpleIdentityServer.Logging;
using System;
namespace SimpleIdentityServer.Host
{
    public class HostingOptions
    {
        /// <summary>
        /// Enable or disable the developer mode
        /// </summary>
        public bool IsDeveloperModeEnabled { get; set; }

        /// <summary>
        /// Migrate the data
        /// </summary>
        public bool IsDataMigrated { get; set; }
    }

    public static class ApplicationBuilderExtensions 
    {        
        #region Public static methods
        
        public static void UseSimpleIdentityServer(this IApplicationBuilder app,
            Action<HostingOptions> hostingCallback,
            ILoggerFactory loggerFactory) 
        {
            if (hostingCallback == null) 
            {
                throw new ArgumentNullException(nameof(hostingCallback));    
            }
            
            var hostingOptions = new HostingOptions();
            hostingCallback(hostingOptions);
            app.UseSimpleIdentityServer(hostingOptions,
                loggerFactory);
        }
        
        public static void UseSimpleIdentityServer(
            this IApplicationBuilder app,
            HostingOptions hostingOptions,
            ILoggerFactory loggerFactory) 
        {
            if (hostingOptions == null)
            {
                throw new ArgumentNullException(nameof(hostingOptions));
            }

            if (hostingOptions.IsDeveloperModeEnabled)
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseSimpleIdentityServerExceptionHandler(new ExceptionHandlerMiddlewareOptions
                {
                    SimpleIdentityServerEventSource = app.ApplicationServices.GetService<ISimpleIdentityServerEventSource>()
                });
            }

            // 1. Configure the IUrlHelper extension
            var httpContextAccessor = app.ApplicationServices.GetRequiredService<IHttpContextAccessor>();
            Extensions.UriHelperExtensions.Configure(httpContextAccessor); 

            // 2. Protect against IFRAME attack
            app.UseXFrame();

            // 3. Migrate all the database
            if (hostingOptions.IsDataMigrated)
            {
                using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
                {
                    var simpleIdentityServerContext = serviceScope.ServiceProvider.GetService<SimpleIdentityServerContext>();
                    // 6C919615
                    simpleIdentityServerContext.Database.EnsureCreated();
                    simpleIdentityServerContext.EnsureSeedData();
                }
            }

            // 4. Add logging
            loggerFactory.AddSerilog();
        }
        
        #endregion        
    }
}