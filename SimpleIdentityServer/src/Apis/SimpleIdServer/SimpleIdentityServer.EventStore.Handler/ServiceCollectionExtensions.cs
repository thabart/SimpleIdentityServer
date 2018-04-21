using Microsoft.Extensions.DependencyInjection;
using SimpleBus.Core;
using SimpleIdentityServer.EventStore.Handler.Handlers;
using System;
using System.Collections.Generic;

namespace SimpleIdentityServer.EventStore.Handler
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddEventStoreBusHandler(this IServiceCollection services, IEnumerable<Type> handlers = null)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddTransient<AuthorizationHandler>();
            services.AddTransient<OpenIdErrorHandler>();
            services.AddTransient<TokenHandler>();
            services.AddTransient<UserInfoHandler>();
            services.AddTransient<IntrospectionHandler>();
            services.AddTransient<RegistrationHandler>();
            var provider = services.BuildServiceProvider();
            var evtHandlerStore = new EvtHandlerStore(provider);
            evtHandlerStore.Register(typeof(AuthorizationHandler));
            evtHandlerStore.Register(typeof(OpenIdErrorHandler));
            evtHandlerStore.Register(typeof(TokenHandler));
            evtHandlerStore.Register(typeof(UserInfoHandler));
            evtHandlerStore.Register(typeof(IntrospectionHandler));
            evtHandlerStore.Register(typeof(RegistrationHandler));
            if (handlers != null)
            {
                foreach (var handler in handlers)
                {
                    evtHandlerStore.Register(handler);
                }
            }

            services.AddSingleton(typeof(IEvtHandlerStore), evtHandlerStore);
            return services;
        }
    }
}
