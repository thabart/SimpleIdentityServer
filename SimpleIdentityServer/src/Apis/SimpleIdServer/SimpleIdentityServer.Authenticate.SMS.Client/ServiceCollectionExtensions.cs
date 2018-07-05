using Microsoft.Extensions.DependencyInjection;
using SimpleIdentityServer.Authenticate.SMS.Client.Factories;
using System;

namespace SimpleIdentityServer.Authenticate.SMS.Client
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAuthenticateSmsClient(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddTransient<IHttpClientFactory, HttpClientFactory>();
            services.AddTransient<ISidSmsAuthenticateClient, SidSmsAuthenticateClient>();
            services.AddTransient<ISendSmsOperation, SendSmsOperation>();
            return services;
        }
    }
}
