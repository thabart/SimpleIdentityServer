using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using SimpleIdentityServer.Scim.Host.Controllers;
using SimpleIdentityServer.Scim.Host.Extensions;
using SimpleIdentityServer.Module;
using System;
using System.Collections.Generic;

namespace SimpleIdentityServer.Scim.Host
{
    public class ScimHostModule : IModule
    {
        public void Init(IDictionary<string, string> properties)
        {
            AspPipelineContext.Instance().ConfigureServiceContext.Initialized += HandleServiceContextInitialized;
            AspPipelineContext.Instance().ConfigureServiceContext.MvcAdded += HandleMvcAdded;
            AspPipelineContext.Instance().ConfigureServiceContext.AuthorizationAdded += HandleAuthorizationAdded;
        }

        private void HandleServiceContextInitialized(object sender, EventArgs e)
        {
            AspPipelineContext.Instance().ConfigureServiceContext.Services.AddScimHost(new ScimServerOptions());
        }
		
        private void HandleMvcAdded(object sender, EventArgs e)
        {
            var services = AspPipelineContext.Instance().ConfigureServiceContext.Services;
            var mvcBuilder = AspPipelineContext.Instance().ConfigureServiceContext.MvcBuilder;
            var assembly = typeof(SchemasController).Assembly;
            var embeddedFileProvider = new EmbeddedFileProvider(assembly);
            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.FileProviders.Add(embeddedFileProvider);
            });

            mvcBuilder.AddApplicationPart(assembly);
        }

        private void HandleAuthorizationAdded(object sender, EventArgs e)
        {
            AspPipelineContext.Instance().ConfigureServiceContext.AuthorizationOptions.AddScimAuthPolicy();
        }
    }
}