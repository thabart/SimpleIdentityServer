using Microsoft.Extensions.DependencyInjection;
using SimpleIdentityServer.TwoFactorAuthentication.Email;
using System;

namespace SimpleIdentityServer.TwoFactorAuthentication.Twilio
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddEmailTwoFactorAuthentication(this IServiceCollection services, TwoFactorEmailOptions twoFactorEmailOptions)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (twoFactorEmailOptions == null)
            {
                throw new ArgumentNullException(nameof(twoFactorEmailOptions));
            }

            services.AddSingleton(twoFactorEmailOptions);
            services.AddTransient<ITwoFactorAuthenticationService, DefaultEmailService>();
            return services;
        }
    }
}
