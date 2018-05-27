using Newtonsoft.Json;
using SimpleIdentityServer.Module.Loader;
using SimpleIdentityServer.Module.Tests.DTOs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Xunit;

namespace SimpleIdentityServer.Module.Tests
{
    public class RestorePackagesFixture
    {
        private ConcurrentBag<string> _installedLibs = new ConcurrentBag<string>();
        private const string _path = @"d:\Projects\Modules";
        private static IEnumerable<string> _nugetSources = new List<string>
        {
            "https://api.nuget.org/v3/index.json",
            "https://www.myget.org/F/advance-ict/api/v3/index.json"
        };

        [Fact]
        public async Task WhenRestorePackageThenPackagesAreInstalled()
        {
            if (!Directory.Exists(_path))
            {
                Directory.CreateDirectory(_path);
            }

            // await RestoreNugetPackage("SimpleIdentityServer.Core.Jwt", "3.0.0-rc6");
            var currentDirectory = Directory.GetCurrentDirectory();
            var configurationFile = Path.Combine(currentDirectory, "config.json");
            var configuration = JsonConvert.DeserializeObject<ProjectConfiguration>(File.ReadAllText(configurationFile));
            foreach (var module in configuration.Modules)
            {
                foreach(var package in module.Packages)
                {
                    if (!_installedLibs.Contains(package.Library))
                    {
                        _installedLibs.Add(package.Library);
                        await RestoreNugetPackage(package.Library, package.Version);
                    }
                }
            }
        }

        private async Task RestoreNugetPackage(string packageName, string version)
        {
            var pkgName = packageName + "." + version;
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

            var serializer2012 = new XmlSerializer(typeof(NugetSpecification2012));
            var serializer2013 = new XmlSerializer(typeof(NugetSpecification2013));
            var nugetDependencies = new List<NugetDependency>();
            XmlDocument xmlDoc = new XmlDocument();
            using (var reader = XmlReader.Create(nuSpecPath))
            {
                xmlDoc.Load(reader);
            }

            using (var sr = new StringReader(xmlDoc.OuterXml))
            {
                List<NugetGroup> groups = null;
                switch (xmlDoc.DocumentElement.NamespaceURI)
                {
                    case "http://schemas.microsoft.com/packaging/2012/06/nuspec.xsd":
                        var nugetSpecification2012 = (NugetSpecification2012)serializer2012.Deserialize(sr);
                        groups = nugetSpecification2012.Metadata.Dependencies.Groups;
                        break;
                    case "http://schemas.microsoft.com/packaging/2013/05/nuspec.xsd":
                        var nugetSpecification2013 = (NugetSpecification2013)serializer2013.Deserialize(sr);
                        groups = nugetSpecification2013.Metadata.Dependencies.Groups;
                        break;
                }
                
                if (groups == null)
                {
                    return;
                }

                foreach (var group in groups)
                {
                    foreach (var dependency in group.Dependencies)
                    {
                        if (!nugetDependencies.Any(nd => nd.Id == dependency.Id))
                        {
                            nugetDependencies.Add(dependency);
                        }
                    }
                }
            }

            var operations = new List<Task>();
            foreach(var nugetDependency in nugetDependencies)
            {
                if (!_installedLibs.Contains(nugetDependency.Id))
                {
                    operations.Add(RestoreNugetPackage(nugetDependency.Id, nugetDependency.Version));
                    _installedLibs.Add(nugetDependency.Id);
                }
            }

            await Task.WhenAll(operations);
        }

        private async Task DownloadNugetPackage(string packageName, string version)
        {
            var pkgSubPath = packageName + "." + version;
            var pkgFileSubPath = pkgSubPath + ".nupkg";
            var pkgPath = GetPath(pkgSubPath);
            var pkgFilePath = GetPath(pkgFileSubPath);
            var httpClient = new HttpClient();
            foreach (var nuget in _nugetSources)
            {
                try
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, nuget);
                    var httpResponse = await httpClient.SendAsync(request).ConfigureAwait(false);
                    var json = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var configurationResponse = JsonConvert.DeserializeObject<NugetConfigurationResponse>(json);
                    var resource = configurationResponse.Resources.FirstOrDefault(r => r.Type.Contains("PackageBaseAddress"));
                    request = new HttpRequestMessage(HttpMethod.Get, resource.Id + packageName + "/index.json");
                    httpResponse = await httpClient.SendAsync(request).ConfigureAwait(false);
                    httpResponse.EnsureSuccessStatusCode();
                    json = await httpResponse.Content.ReadAsStringAsync();
                    var flatContainerResponse = JsonConvert.DeserializeObject<NugetFlatContainerResponse>(json);
                    if (flatContainerResponse == null || !flatContainerResponse.Versions.Contains(version))
                    {
                        continue;
                    }

                    request = new HttpRequestMessage(HttpMethod.Get, resource.Id + packageName + "/" + version + "/" + packageName + "." + version + ".nupkg");
                    using (var contentStream = await (await httpClient.SendAsync(request)).Content.ReadAsStreamAsync())
                    {
                        using (var stream = new FileStream(pkgFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            await contentStream.CopyToAsync(stream);
                        }
                    }

                    ZipFile.ExtractToDirectory(pkgFilePath, pkgPath);
                    File.Delete(pkgFilePath);
                    Trace.WriteLine($"The nuget package {packageName + "." + version} is installed");
                }
                catch (Exception)
                {

                }
            }
        }

        private static string GetPath(string subPath)
        {
            return Path.Combine(_path, subPath);
        }
    }
}
