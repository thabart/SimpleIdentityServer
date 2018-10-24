using SimpleIdentityServer.Module;
using System;
using System.Collections.Generic;

namespace SimpleIdentityServer.UserManagement
{
    public class UserManagementModule : IModule
    {
        public void Init(IDictionary<string, string> options)
        {
            AspPipelineContext.Instance().ConfigureServiceContext.MvcAdded += HandleMvcAdded;
            AspPipelineContext.Instance().ApplicationBuilderContext.RouteConfigured += HandleConfigureRoute;
        }

        private void HandleMvcAdded(object sender, EventArgs e)
        {
            var configureServiceContext = AspPipelineContext.Instance().ConfigureServiceContext;
            configureServiceContext.Services.AddUserManagement(configureServiceContext.MvcBuilder);
        }

        private void HandleConfigureRoute(object sender, EventArgs e)
        {
            AspPipelineContext.Instance().ApplicationBuilderContext.RouteBuilder.UseUserManagement();
        }
    }
}