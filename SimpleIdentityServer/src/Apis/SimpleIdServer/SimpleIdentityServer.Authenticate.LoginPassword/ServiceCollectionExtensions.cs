using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using SimpleIdentityServer.Authenticate.Basic;
using SimpleIdentityServer.Authenticate.LoginPassword.Controllers;
using SimpleIdentityServer.Authenticate.LoginPassword.Services;
using SimpleIdentityServer.Core.Services;
using System;
using System.Reflection;

namespace SimpleIdentityServer.Authenticate.LoginPassword
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddLoginPasswordAuthentication(this IServiceCollection services, IMvcBuilder mvcBuilder, BasicAuthenticateOptions basicAuthenticateOptions)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (mvcBuilder == null)
            {
                throw new ArgumentNullException(nameof(mvcBuilder));
            }

            if (basicAuthenticateOptions == null)
            {
                throw new ArgumentNullException(nameof(basicAuthenticateOptions));
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
            services.AddSingleton(basicAuthenticateOptions);
            services.AddTransient<IAuthenticateResourceOwnerService, PasswordAuthenticateResourceOwnerService>();
            mvcBuilder.AddApplicationPart(assembly);
            return services;
        }
    }
}
