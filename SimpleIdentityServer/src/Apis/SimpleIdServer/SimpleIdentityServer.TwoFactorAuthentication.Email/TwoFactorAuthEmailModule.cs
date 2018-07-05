using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using SimpleIdentityServer.Module;
using System;
using System.Collections.Generic;

namespace SimpleIdentityServer.TwoFactorAuthentication.Email
{
    public class TwoFactorAuthEmailModule : IModule
    {
        private const string EmailFromName = "EmailFromName";
        private const string EmailFromAddress = "EmailFromAddress";
        private const string EmailSubject = "EmailSubject";
        private const string EmailBody = "EmailBody";
        private const string EmailSmtpHost = "EmailSmtpHost";
        private const string EmailSmtpPort = "EmailSmtpPort";
        private const string EmailSmtpUseSsl = "EmailSmtpUseSsl";
        private const string EmailUserName = "EmailUserName";
        private const string EmailPassword = "EmailPassword";

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
            // TODO : IMPLEMENT.
        }

        public ModuleUIDescriptor GetModuleUI()
        {
            return null;
        }

        public IEnumerable<string> GetOptionKeys()
        {
            return new[]
            {
                EmailFromName,
                EmailFromAddress,
                EmailSubject,
                EmailBody,
                EmailSmtpHost,
                EmailSmtpPort,
                EmailSmtpUseSsl,
                EmailUserName,
                EmailPassword
            };
        }

        private static TwoFactorEmailOptions GetOptions(IDictionary<string, string> options)
        {
            var emailServiceOptions = new TwoFactorEmailOptions
            {
                EmailBody = options.TryGetValue(EmailBody),
                EmailFromAddress = options.TryGetValue(EmailFromAddress),
                EmailFromName = options.TryGetValue(EmailFromName),
                EmailPassword = options.TryGetValue(EmailPassword),
                EmailSmtpHost = options.TryGetValue(EmailSmtpHost),
                EmailSubject = options.TryGetValue(EmailSubject),
                EmailUserName = options.TryGetValue(EmailUserName)
            };

            int port;
            bool useSsl = false;
            if (options.TryGetValue(EmailSmtpPort, out port))
            {
                emailServiceOptions.EmailSmtpPort = port;

            }

            if (options.TryGetValue(EmailSmtpUseSsl, out useSsl))
            {
                emailServiceOptions.EmailSmtpUseSsl = useSsl;
            }
            
            return emailServiceOptions;
        }
    }
}