using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using SimpleIdentityServer.Module;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

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

                var files = Directory.GetFiles(path, "*.dll");
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
        public void ConfigureServices(IServiceCollection services, IMvcBuilder mvcBuilder, IHostingEnvironment env)
        {
            foreach(var module in _modules)
            {
                module.ConfigureServices(services, mvcBuilder, env);
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
            // TH : Load those assemblies ?
            var directoryToWatch = Path.Combine(Directory.GetCurrentDirectory(), _modulePath);
            _watcher.Path = directoryToWatch;
            _watcher.Filter = "*.dll";
            _watcher.Deleted += Deleted;
            _watcher.Renamed += Renamed;
            _watcher.Created += Created;
            _watcher.EnableRaisingEvents = true;
            _watcher.IncludeSubdirectories = true;
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

            return null;
        }

        private void Deleted(object sender, FileSystemEventArgs e)
        {
            string s = "";
        }

        private void Created(object sender, FileSystemEventArgs e)
        {
            string s = "";
        }

        private void Renamed(object sender, RenamedEventArgs e)
        {
            string s = "";
        }
    }
}
