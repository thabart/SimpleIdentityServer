using SimpleIdentityServer.Module;
using System;
using System.Collections.Generic;

namespace SimpleIdServer.Storage
{
    public class InMemoryStorageModule : IModule
    {
        public void Init(IDictionary<string, string> options)
        {
            AspPipelineContext.Instance().ConfigureServiceContext.Initialized += HandleServiceContextInitialized;
        }

        private void HandleServiceContextInitialized(object sender, EventArgs e)
        {
            AspPipelineContext.Instance().ConfigureServiceContext.Services.AddStorage(opts => opts.UseInMemoryStorage());
        }
    }
}
