using Microsoft.Extensions.DependencyInjection;
using SimpleIdentityServer.Authenticate.SMS.Client.Factories;
using System;

namespace SimpleIdentityServer.Authenticate.SMS.Client
{
    public interface ISidSmsAuthenticateClientFactory
    {
        ISidSmsAuthenticateClient GetClient();
    }

    public class SidSmsAuthenticateClientFactory : ISidSmsAuthenticateClientFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public SidSmsAuthenticateClientFactory()
        {
            var services = new ServiceCollection();
            RegisterDependencies(services);
            _serviceProvider = services.BuildServiceProvider();
        }

        public ISidSmsAuthenticateClient GetClient()
        {
            var result = _serviceProvider.GetService<ISidSmsAuthenticateClient>();
            return result;
        }

        private static void RegisterDependencies(IServiceCollection services)
        {
            services.AddTransient<IHttpClientFactory, HttpClientFactory>();
            services.AddTransient<ISidSmsAuthenticateClient, SidSmsAuthenticateClient>();
            services.AddTransient<ISendSmsOperation, SendSmsOperation>();
        }
    }
}