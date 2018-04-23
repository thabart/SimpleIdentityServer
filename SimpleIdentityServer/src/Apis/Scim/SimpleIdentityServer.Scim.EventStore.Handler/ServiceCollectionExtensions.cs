using Microsoft.Extensions.DependencyInjection;
using SimpleBus.Core;
using SimpleIdentityServer.Scim.EventStore.Handler.Handlers;
using System;
using System.Collections.Generic;

namespace SimpleIdentityServer.Scim.EventStore.Handler
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddEventStoreBusHandler(this IServiceCollection services, IEnumerable<Type> handlers = null)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddTransient<GroupHandler>();
            services.AddTransient<ScimErrorHandler>();
            services.AddTransient<UserHandler>();
            var provider = services.BuildServiceProvider();
            var evtHandlerStore = new EvtHandlerStore(provider);
            evtHandlerStore.Register(typeof(GroupHandler));
            evtHandlerStore.Register(typeof(ScimErrorHandler));
            evtHandlerStore.Register(typeof(UserHandler));
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
