using SimpleIdentityServer.Module;
using System.Collections.Generic;

namespace SimpleIdentityServer.Authenticate.SMS
{
    public class SmsModule : IModule
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
            configureServiceContext.Services.AddSmsAuthentication(configureServiceContext.MvcBuilder, GetOptions());
        }

        private void HandleRouteConfigured(object sender, System.EventArgs e)
        {
            var applicationBuilderContext = AspPipelineContext.Instance().ApplicationBuilderContext;
            applicationBuilderContext.RouteBuilder.UseSmsAuthentication();
        }

        private SmsAuthenticationOptions GetOptions()
        {
            var result = new SmsAuthenticationOptions();
            if (_properties != null)
            {
                var message = string.Empty;
                var accountSid = string.Empty;
                var authToken = string.Empty;
                var fromNumber = string.Empty;
                if (_properties.TryGetValue("Message", out message))
                {
                    result.Message = message;
                }

                if (_properties.TryGetValue("AccountSid", out accountSid))
                {
                    result.TwilioSmsCredentials.AccountSid = accountSid;
                }

                if (_properties.TryGetValue("AuthToken", out authToken))
                {
                    result.TwilioSmsCredentials.AuthToken = authToken;
                }

                if(_properties.TryGetValue("FromNumber", out fromNumber))
                {
                    result.TwilioSmsCredentials.FromNumber = fromNumber;
                }

                result.ClaimsIncludedInUserCreation = _properties.TryGetArr("ClaimsIncludedInUserCreation");
            }

            return result;
        }
    }
}
