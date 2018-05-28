using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using SimpleIdentityServer.Module.Loader.Exceptions;
using SimpleIdentityServer.Module.Loader.Nuget;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace SimpleIdentityServer.Module.Loader
{
    public interface IModuleLoader
    {
        void Initialize();
        Task RestorePackages();
        void LoadModules();
        void ConfigureServices(IServiceCollection services, IMvcBuilder mvcBuilder, IHostingEnvironment env, Dictionary<string, string> opts = null);
        void Configure(IRouteBuilder routes);
        void Configure(IApplicationBuilder app);
    }

    internal sealed class ModuleLoader : IModuleLoader
    {
        private readonly INugetClient _nugetClient;
        private readonly ModuleLoaderOptions _options;
        private const string _configFile = "config.json";
        private IEnumerable<IModule> _modules;
        private bool _isInitialized = false;
        private ProjectConfiguration _projectConfiguration;
        private ConcurrentBag<string> _installedLibs;

        public ModuleLoader(INugetClient nugetClient, ModuleLoaderOptions options)
        {
            if (nugetClient == null)
            {
                throw new ArgumentNullException(nameof(nugetClient));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _nugetClient = nugetClient;
            _options = options;
        }

        /// <summary>
        /// Initialize the module loader.
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized)
            {
                throw new ModuleLoaderInternalException("the loader is already initialized");
            }

            AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve;
            if (!Directory.Exists(_options.ModulePath))
            {
                throw new DirectoryNotFoundException(nameof(_options.ModulePath));
            }

            if (_options.NugetSources == null || !_options.NugetSources.Any())
            {
                throw new ModuleLoaderInternalException("At least one nuget sources must be specified");
            }

            var configurationFilePath = Path.Combine(_options.ModulePath, _configFile);
            if (!File.Exists(configurationFilePath))
            {
                throw new FileNotFoundException(configurationFilePath);
            }

            var json = File.ReadAllText(configurationFilePath);
            _projectConfiguration = JsonConvert.DeserializeObject<ProjectConfiguration>(json);
            if (_projectConfiguration == null)
            {
                throw new ModuleLoaderConfigurationException($"{configurationFilePath} is not a valid configuration file");
            }

            _isInitialized = true;
        }
        
        /// <summary>
        /// Restore the packages.
        /// </summary>
        /// <returns></returns>
        public async Task RestorePackages()
        {
            if (!_isInitialized)
            {
                throw new ModuleLoaderInternalException("the loader is not initialized");
            }

            if (_projectConfiguration.Modules == null || !_projectConfiguration.Modules.Any())
            {
                return;
            }

            _installedLibs = new ConcurrentBag<string>();
            foreach (var module in _projectConfiguration.Modules)
            {
                if (module.Packages == null || !module.Packages.Any())
                {
                    continue;
                }

                foreach(var package in module.Packages)
                {
                    _installedLibs.Add($"{package.Library}.{package.Version}");
                    await RestorePackages(package.Library, package.Version);
                }
            }
        }

        /// <summary>
        /// Returns the list of loaded modules.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IModule> GetModules()
        {
            return _modules;
        }

        /// <summary>
        /// Load the modules.
        /// </summary>
        public void LoadModules()
        {
            /*
            _modules = new List<IModule>();

            var result = new List<IModule>();
            var currentDirectory = Directory.GetCurrentDirectory();
            var subDirectories = Directory.GetDirectories(_modulePath);
            foreach (var module in configuration.Modules)
            {
                var subDirectory = Path.Combine(_modulePath, module.ModuleName);
                if (!Directory.Exists(subDirectory))
                {
                    continue;
                }


                foreach (var package in module.Packages)
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
            */
        }

        /// <summary>
        /// Register the services.
        /// </summary>
        public void ConfigureServices(IServiceCollection services, IMvcBuilder mvcBuilder, IHostingEnvironment env, Dictionary<string, string> opts = null)
        {
            foreach (var module in _modules)
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
            foreach (var module in _modules)
            {
                module.Configure(app);
            }
        }

        #region Private methods

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
            /*
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
            */
        }

        private async Task RestorePackages(string packageName, string version)
        {
            if (string.IsNullOrWhiteSpace(packageName) || string.IsNullOrWhiteSpace(version))
            {
                return;
            }

            var pkgName = $"{packageName}.{version}";
            var packagePath = GetPath(pkgName);
            var nuSpecPath = GetPath(Path.Combine(pkgName, packageName + ".nuspec"));
            if (!Directory.Exists(packagePath))
            {
                await DownloadNugetPackage(packageName, version);
            }
            
            if (!File.Exists(nuSpecPath))
            {
                return;
            }

            var nugetDependencies = new List<NugetDependency>();
            var xmlDoc = new XmlDocument();
            using (var reader = XmlReader.Create(nuSpecPath))
            {
                xmlDoc.Load(reader);
            }

            List<NugetGroup> groups = null;
            using (var strReader = new StringReader(xmlDoc.OuterXml))
            {
                switch (xmlDoc.DocumentElement.NamespaceURI)
                {
                    case "http://schemas.microsoft.com/packaging/2012/06/nuspec.xsd":
                        var serializer2012 = new XmlSerializer(typeof(NugetSpecification2012));
                        var nugetSpecification2012 = (NugetSpecification2012)serializer2012.Deserialize(strReader);
                        groups = nugetSpecification2012.Metadata.Dependencies.Groups;
                        break;
                    case "http://schemas.microsoft.com/packaging/2013/05/nuspec.xsd":
                        var serializer2013 = new XmlSerializer(typeof(NugetSpecification2013));
                        var nugetSpecification2013 = (NugetSpecification2013)serializer2013.Deserialize(strReader);
                        groups = nugetSpecification2013.Metadata.Dependencies.Groups;
                        break;
                }
            }

            if (groups == null)
            {
                return;
            }

            foreach(var group in groups)
            {
                if (group.Dependencies == null)
                {
                    continue;
                }

                foreach(var dependency in group.Dependencies)
                {
                    if (!nugetDependencies.Any(nd => $"{nd.Id}.{nd.Version}" == $"{dependency.Id}.{dependency.Version}"))
                    {
                        nugetDependencies.Add(dependency);
                    }
                }
            }

            var operations = new List<Task>();
            foreach(var nugetDependency in nugetDependencies)
            {
                if (!_installedLibs.Contains($"{nugetDependency.Id}.{nugetDependency.Version}"))
                {
                    operations.Add(RestorePackages(nugetDependency.Id, nugetDependency.Version));
                    _installedLibs.Add($"{nugetDependency.Id}.{nugetDependency.Version}");
                }
            }

            await Task.WhenAll(operations);
        }

        private async Task DownloadNugetPackage(string packageName, string version)
        {
            var pkgSubPath = $"{packageName}.{version}";
            var pkgFileSubPath = pkgSubPath + ".nupkg";
            var pkgPath = GetPath(pkgSubPath);
            var pkgFilePath = GetPath(pkgFileSubPath);
            foreach (var nugetSource in _options.NugetSources)
            {
                try
                {
                    Uri uriResult;
                    if (!Uri.TryCreate(nugetSource, UriKind.Absolute, out uriResult))
                    {
                        continue;
                    }

                    if (Directory.Exists(nugetSource))
                    {
                        var files = Directory.GetFiles(nugetSource, pkgFileSubPath);
                        if (files == null || !files.Any())
                        {
                            continue;
                        }

                        File.Copy(files.First(), pkgFilePath);
                    }
                    else
                    {

                        var configuration = await _nugetClient.GetConfiguration(nugetSource);
                        if (configuration == null)
                        {
                            continue;
                        }

                        var pkgBaseAdr = configuration.Resources.FirstOrDefault(r => r.Type.Contains("PackageBaseAddress"));
                        if (pkgBaseAdr == null)
                        {
                            continue;
                        }

                        var flatContainerUrl = $"{pkgBaseAdr.Id}{packageName}/index.json";
                        var flatContainerResponse = await _nugetClient.GetNugetFlatContainer(flatContainerUrl, packageName);
                        if (flatContainerResponse == null || !flatContainerResponse.Versions.Contains(version))
                        {
                            continue;
                        }

                        using (var contentStream = await _nugetClient.DownloadNugetPackage(pkgBaseAdr.Id, packageName, version))
                        {
                            using (var stream = new FileStream(pkgFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                            {
                                await contentStream.CopyToAsync(stream);
                            }
                        }
                    }

                    ZipFile.ExtractToDirectory(pkgFilePath, pkgPath);
                    File.Delete(pkgFilePath);
                    return;
                }
                catch (Exception)
                {

                }
            }
        }

        private string GetPath(string subPath)
        {
            return Path.Combine(_options.ModulePath, subPath);
        }

        #endregion
    }
}
