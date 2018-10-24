using SimpleIdentityServer.Authenticate.Basic;
using SimpleIdentityServer.Module;
using System.Collections.Generic;

namespace SimpleIdentityServer.Authenticate.LoginPassword
{
    public class LoginPasswordModule : IModule
    {
        private IDictionary<string, string> _properties;

        public void Init(IDictionary<string, string> properties)
        {
            _properties = properties;
            AspPipelineContext.Instance().ConfigureServiceContext.MvcAdded += HandleMvcAdded;
            AspPipelineContext.Instance().ApplicationBuilderContext.RouteConfigured += HandleRouteConfigured;
        }

        private void HandleMvcAdded(object sender, System.EventArgs e)
        {
            var configureServiceContext = AspPipelineContext.Instance().ConfigureServiceContext;
            configureServiceContext.Services.AddLoginPasswordAuthentication(configureServiceContext.MvcBuilder, GetOptions());
        }

        private void HandleRouteConfigured(object sender, System.EventArgs e)
        {
            var applicationBuilderContext = AspPipelineContext.Instance().ApplicationBuilderContext;
            applicationBuilderContext.RouteBuilder.UseLoginPasswordAuthentication();
        }

        private BasicAuthenticateOptions GetOptions()
        {
            var result = new BasicAuthenticateOptions();
            if (_properties != null)
            {
                result.ClaimsIncludedInUserCreation = _properties.TryGetArr("ClaimsIncludedInUserCreation");                
            }

            return result;
        }
    }
}
