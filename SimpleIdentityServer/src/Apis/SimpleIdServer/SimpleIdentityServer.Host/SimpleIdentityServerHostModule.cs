using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using SimpleIdentityServer.Api.Controllers.Api;
using SimpleIdentityServer.Module;
using System;
using System.Collections.Generic;

namespace SimpleIdentityServer.Host
{
    public class SimpleIdentityServerHostModule : IModule
    {
        private IDictionary<string, string> _properties;

        public void Init(IDictionary<string, string> properties)
        {
            _properties = properties;
            AspPipelineContext.Instance().ConfigureServiceContext.Initialized += HandleServiceContextInitialized;
            AspPipelineContext.Instance().ConfigureServiceContext.MvcAdded += HandleMvcAdded;
            AspPipelineContext.Instance().ConfigureServiceContext.AuthorizationAdded += HandleAuthorizationAdded;
            AspPipelineContext.Instance().ApplicationBuilderContext.Initialized += HandleApplicationBuilderInitialized;
        }

        private void HandleServiceContextInitialized(object sender, EventArgs e)
        {
            var services = AspPipelineContext.Instance().ConfigureServiceContext.Services;
            services.AddOpenIdApi(o => { });
        }

        private void HandleMvcAdded(object sender, EventArgs e)
        {
            var services = AspPipelineContext.Instance().ConfigureServiceContext.Services;
            var mvcBuilder = AspPipelineContext.Instance().ConfigureServiceContext.MvcBuilder;
            var assembly = typeof(AuthorizationController).Assembly;
            var embeddedFileProvider = new EmbeddedFileProvider(assembly);
            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.FileProviders.Add(embeddedFileProvider);
            });

            mvcBuilder.AddApplicationPart(assembly);
        }

        private void HandleAuthorizationAdded(object sender, EventArgs e)
        {
            AspPipelineContext.Instance().ConfigureServiceContext.AuthorizationOptions.AddOpenIdSecurityPolicy(CookieAuthenticationDefaults.AuthenticationScheme);
        }

        private void HandleApplicationBuilderInitialized(object sender, EventArgs e)
        {
            AspPipelineContext.Instance().ApplicationBuilderContext.App.UseOpenIdApi(new IdentityServerOptions());
        }
    }
}
