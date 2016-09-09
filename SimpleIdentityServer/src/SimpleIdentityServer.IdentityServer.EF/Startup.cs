// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.EntityFramework.DbContexts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace IdentityServer4.EntityFramework
{
    public class Startup
    {
        public IConfigurationRoot Configuration { get; set; }

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;
            var connectionString = Configuration["Data:DefaultConnection:ConnectionString"];
            services.AddEntityFramework()
               .AddDbContext<ConfigurationDbContext>(options =>
                   options.UseSqlServer(connectionString, opt =>
                       opt.MigrationsAssembly(migrationsAssembly)));
            services.AddEntityFramework()
               .AddDbContext<PersistedGrantDbContext>(options =>
                   options.UseSqlServer(connectionString, opt =>
                       opt.MigrationsAssembly(migrationsAssembly)));
        }

        public static void Main(string[] args) { }
    }
}
