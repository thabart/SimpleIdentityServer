using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using SimpleIdentityServer.Authenticate.SMS.Actions;
using SimpleIdentityServer.Authenticate.SMS.Controllers;
using SimpleIdentityServer.Authenticate.SMS.Services;
using SimpleIdentityServer.Core.Services;
using SimpleIdentityServer.Twilio.Client;
using System;

namespace SimpleIdentityServer.Authenticate.SMS
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSmsAuthentication(this IServiceCollection services, 
            IMvcBuilder mvcBuilder, IHostingEnvironment hosting, SmsAuthenticationOptions smsAuthenticationOptions)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (mvcBuilder == null)
            {
                throw new ArgumentNullException(nameof(mvcBuilder));
            }

            if (smsAuthenticationOptions == null)
            {
                throw new ArgumentNullException(nameof(smsAuthenticationOptions));
            }

            var assembly = typeof(AuthenticateController).Assembly;
            var embeddedFileProvider = new EmbeddedFileProvider(assembly);
            services.Configure<RazorViewEngineOptions>(opts =>
            {
                opts.FileProviders.Add(embeddedFileProvider);
            });
            services.AddSingleton(smsAuthenticationOptions);
            services.AddTransient<ITwilioClient, TwilioClient>();
            services.AddTransient<ISmsAuthenticationOperation, SmsAuthenticationOperation>();
            services.AddTransient<IGenerateAndSendSmsCodeOperation, GenerateAndSendSmsCodeOperation>();
            services.AddTransient<IAuthenticateResourceOwnerService, SmsAuthenticateResourceOwnerService>();
            mvcBuilder.AddApplicationPart(assembly);
            return services;
        }
    }
}
