using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using SimpleIdentityServer.Module;
using System;
using System.Collections.Generic;

namespace SimpleIdentityServer.TwoFactorAuthentication.Twilio
{
    public class TwoFactorAuthTwilioModule : IModule
    {
        private const string TwilioAccountSid = "TwilioAccountSid";
        private const string TwilioAuthToken = "TwilioAuthToken";
        private const string TwilioFromNumber = "TwilioFromNumber";
        private const string TwilioMessage = "TwilioMessage";

        public void Configure(IApplicationBuilder applicationBuilder)
        {
        }

        public void Configure(IRouteBuilder routeBuilder)
        {
        }

        public void ConfigureAuthentication(Microsoft.AspNetCore.Authentication.AuthenticationBuilder authBuilder, IDictionary<string, string> options = null)
        {
        }

        public void ConfigureAuthorization(AuthorizationOptions authorizationOptions, IDictionary<string, string> options = null)
        {
        }

        public void ConfigureServices(IServiceCollection services, IMvcBuilder mvcBuilder = null, IHostingEnvironment env = null, IDictionary<string, string> options = null, IEnumerable<ModuleUIDescriptor> moduleUiDescriptors = null)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            var opts = GetOptions(options);
            var twoFactor = new DefaultTwilioSmsService(opts);
            TwoFactorServiceStore.Instance().Add("SMS", twoFactor);
        }

        public ModuleUIDescriptor GetModuleUI()
        {
            return null;
        }

        public IEnumerable<string> GetOptionKeys()
        {
            return new[]
            {
                TwilioAccountSid,
                TwilioAuthToken,
                TwilioFromNumber,
                TwilioMessage
            };
        }

        private static TwilioOptions GetOptions(IDictionary<string, string> options)
        {
            var twilioOptions = new TwilioOptions
            {
                TwilioAccountSid = options.TryGetValue(TwilioAccountSid),
                TwilioAuthToken = options.TryGetValue(TwilioAuthToken),
                TwilioFromNumber = options.TryGetValue(TwilioFromNumber),
                TwilioMessage = options.TryGetValue(TwilioMessage)
            };
            return twilioOptions;
        }
    }
}