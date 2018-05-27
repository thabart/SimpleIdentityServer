using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using SimpleIdentityServer.Module;
using SimpleIdentityServer.Module.Loader;
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
        private const string _configFile = "config.json";
        private const string _configResolveFile = "config.resolve.json";
        private ProjectResolvedConfiguration _projectResolvedConfiguration;
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

            if (!File.Exists(Path.Combine(_modulePath, _configFile)))
            {
                return;
            }

            if (!File.Exists(Path.Combine(_modulePath, _configResolveFile)))
            {
                return;
            }

            var txt = File.ReadAllText(Path.Combine(_modulePath, _configFile));
            var configuration = JsonConvert.DeserializeObject<ProjectConfiguration>(txt);
            _projectResolvedConfiguration = JsonConvert.DeserializeObject<ProjectResolvedConfiguration>(File.ReadAllText(Path.Combine(_modulePath, _configResolveFile)));
            var result = new List<IModule>();
            var currentDirectory = Directory.GetCurrentDirectory();
            var subDirectories = Directory.GetDirectories(_modulePath);
            foreach(var module in configuration.Modules)
            {
                var subDirectory = Path.Combine(_modulePath, module.ModuleName);
                if (!Directory.Exists(subDirectory))
                {
                    continue;
                }


                foreach(var package in module.Packages)
                {
                    var libPath = Path.Combine(subDirectory, package.Library);
                    var libFkPath = Path.Combine(libPath, "net461"); // TODO : Retrieve the correct framework version (netcoreapp2.0 or net461)
                    if (!Directory.Exists(libPath) || !Directory.Exists(libFkPath))
                    {
                        continue;
                    }

                    var files = Directory.GetFiles(libFkPath, "SimpleIdentityServer.*.dll");
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

            var requestingName = args.RequestingAssembly.ManifestModule.Name.Replace(".dll", "");
            var requestingLib = _projectResolvedConfiguration.Libraries.FirstOrDefault(l => l.Name == requestingName);
            var requestedName = args.Name.Split(',').First();
            var requestedLib = _projectResolvedConfiguration.Libraries.FirstOrDefault(l => l.Name == requestedName);
            var modulePath = Path.Combine(_modulePath, "lib", requestedName, "net461", requestedName + ".dll");
            if (requestingLib == null || requestedLib == null ||
                !File.Exists(modulePath))
            {
                return null;
            }

            modulePath = Path.Combine(Directory.GetCurrentDirectory(), modulePath);
            return Assembly.LoadFile(modulePath);
        }
    }
}
