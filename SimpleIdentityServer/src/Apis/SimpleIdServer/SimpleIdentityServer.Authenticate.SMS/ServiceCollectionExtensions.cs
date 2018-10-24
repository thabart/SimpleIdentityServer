using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using SimpleIdentityServer.Authenticate.SMS.Actions;
using SimpleIdentityServer.Authenticate.SMS.Controllers;
using SimpleIdentityServer.Authenticate.SMS.Services;
using SimpleIdentityServer.Core.Services;
using SimpleIdentityServer.Twilio.Client;
using System;
using System.Reflection;

namespace SimpleIdentityServer.Authenticate.SMS
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSmsAuthentication(this IServiceCollection services,  IMvcBuilder mvcBuilder, SmsAuthenticationOptions smsAuthenticationOptions)
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
                opts.CompilationCallback = (context) =>
                {
                    var assm = MetadataReference.CreateFromFile(Assembly.Load("SimpleIdentityServer.Authenticate.Basic").Location);
                    context.Compilation = context.Compilation.AddReferences(assm);
                };
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
