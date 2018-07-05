using Microsoft.Extensions.DependencyInjection;
using System;

namespace SimpleIdentityServer.TwoFactorAuthentication.Twilio
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddTwoFactorSmsAuthentication(this IServiceCollection services, TwoFactorTwilioOptions twilioOptions)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (twilioOptions == null)
            {
                throw new ArgumentNullException(nameof(twilioOptions));
            }

            services.AddSingleton(twilioOptions);
            services.AddTransient<ITwoFactorAuthenticationService, DefaultTwilioSmsService>();
            return services;
        }
    }
}
