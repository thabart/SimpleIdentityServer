using SimpleIdentityServer.Module;
using System;
using System.Collections.Generic;

namespace SimpleIdentityServer.Shell
{
    public class ShellModule : IModule
    {
        public void Init(IDictionary<string, string> options)
        {
            AspPipelineContext.Instance().ConfigureServiceContext.MvcAdded += HandleMvcAdded;
            AspPipelineContext.Instance().ApplicationBuilderContext.Initialized += HandleApplicationBuilderInitialized;
            AspPipelineContext.Instance().ApplicationBuilderContext.RouteConfigured += HandleRouteConfigured;
        }

        private void HandleApplicationBuilderInitialized(object sender, EventArgs e)
        {
            var applicationBuilderContext = AspPipelineContext.Instance().ApplicationBuilderContext;
            applicationBuilderContext.App.UseShellStaticFiles();
        }

        private void HandleMvcAdded(object sender, EventArgs e)
        {
            var configureServiceContext = AspPipelineContext.Instance().ConfigureServiceContext;
            configureServiceContext.Services.AddBasicShell(configureServiceContext.MvcBuilder);
        }

        private void HandleRouteConfigured(object sender, EventArgs e)
        {
            AspPipelineContext.Instance().ApplicationBuilderContext.RouteBuilder.UseShell();
        }
    }
}
