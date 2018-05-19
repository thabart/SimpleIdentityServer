using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using SimpleIdentityServer.Module;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace SimpleIdentityServer.Startup
{
    public class ModuleLoader
    {
        private const string _modulePath = "Modules";
        private IEnumerable<IModule> _modules;
        private FileSystemWatcher _watcher;

        public ModuleLoader()
        {
            _modules = new List<IModule>();
            _watcher = new FileSystemWatcher();
            Initialize();
        }

        public IEnumerable<IModule> GetModules()
        {
            return _modules;
        }

        /// <summary>
        /// Load all the modules.
        /// </summary>
        public void LoadModules()
        {
            _modules = new List<IModule>();
            if (!Directory.Exists(_modulePath))
            {
                return;
            }

            var result = new List<IModule>();
            var currentDirectory = Directory.GetCurrentDirectory();
            var subDirectories = Directory.GetDirectories(_modulePath);
            foreach (var subDirectory in subDirectories)
            {
                var path = Path.Combine(subDirectory, "net461"); // TODO : Retrieve the correct framework version (netcoreapp2.0 or net461)
                if (!Directory.Exists(path))
                {
                    continue;
                }

                var files = Directory.GetFiles(path, "SimpleIdentityServer.*.dll");
                foreach (var file in files)
                {
                    var assm = Assembly.LoadFile(Path.Combine(currentDirectory, file));
                    var modules = assm.GetExportedTypes().Where(t => typeof(IModule).IsAssignableFrom(t));
                    if (modules == null || !modules.Any() || modules.Count() != 1)
                    {
                        continue;
                    }

                    result.Add((IModule)Activator.CreateInstance(modules.First()));
                }
            }

            _modules = result;
        }

        /// <summary>
        /// Register the services.
        /// </summary>
        public void ConfigureServices(IServiceCollection services, IMvcBuilder mvcBuilder, IHostingEnvironment env, Dictionary<string, string> opts = null)
        {
            foreach(var module in _modules)
            {
                module.ConfigureServices(services, mvcBuilder, env, opts);
            }
        }

        public void Configure(IRouteBuilder routes)
        {
            foreach (var module in _modules)
            {
                module.Configure(routes);
            }
        }

        public void Configure(IApplicationBuilder app)
        {
            foreach(var module in _modules)
            {
                module.Configure(app);
            }
        }

        private void Initialize()
        {
            AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve;
        }

        private Assembly AssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (args.Name.Contains(".resources"))
            {
                return null;
            }

            var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName == args.Name);
            if (assembly != null)
            {
                return assembly;
            }

            var location = Path.Combine(Path.GetDirectoryName(args.RequestingAssembly.Location), args.Name);
            return Assembly.LoadFile(location);
        }
    }
}
