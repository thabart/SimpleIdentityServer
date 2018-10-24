using SimpleIdentityServer.Module;
using System;
using System.Collections.Generic;

namespace SimpleIdentityServer.AccountFilter.Basic
{
    public class BasicAccountFilterModule : IModule
    {
        public void Init(IDictionary<string, string> properties)
        {
            AspPipelineContext.Instance().ConfigureServiceContext.MvcAdded += HandleMvcAdded;
        }

        private void HandleMvcAdded(object sender, EventArgs eventArgs)
        {
            var serviceContext = AspPipelineContext.Instance().ConfigureServiceContext;
            serviceContext.Services.AddAccountFilter(serviceContext.MvcBuilder);
        }
    }
}
