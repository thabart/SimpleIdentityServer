using SimpleIdentityServer.Module.Loader.Exceptions;
using SimpleIdentityServer.Module.Loader.Factories;
using SimpleIdentityServer.Module.Loader.Nuget;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Module.Loader.Tests
{
    public class ModuleLoaderFixture
    {
        [Fact]
        public void WhenPassingNullParameterToConstructorThenExceptionIsThrown()
        {
            Assert.Throws<ArgumentNullException>(() => new ModuleLoader(null, null));
            Assert.Throws<ArgumentNullException>(() => new ModuleLoader(new NugetClient(new HttpClientFactory()), null));
        }

        #region Initialize

        [Fact]
        public void WhenInitializeModuleLoaderAndModulePathDoesntExistThenExceptionIsThrown()
        {
            // ARRANGE
            var nugetClient = new NugetClient(new HttpClientFactory());
            var options = new ModuleLoaderOptions
            {
                ModulePath = "bad_path"
            };
            var moduleLoader = new ModuleLoader(nugetClient, options);

            // ACT
            var exception = Assert.Throws<DirectoryNotFoundException>(() => moduleLoader.Initialize());

            // ASSERT
            Assert.NotNull(exception);
        }

        [Fact]
        public void WhenInitializeModuleLoaderAndNugetSourcesIsNullThenExceptionIsThrown()
        {
            // ARRANGE
            var nugetClient = new NugetClient(new HttpClientFactory());
            var options = new ModuleLoaderOptions
            {
                ModulePath = Directory.GetCurrentDirectory(),
                NugetSources = null
            };
            var moduleLoader = new ModuleLoader(nugetClient, options);

            // ACT
            var exception = Assert.Throws<ModuleLoaderInternalException>(() => moduleLoader.Initialize());

            // ASSERT
            Assert.NotNull(exception);
        }

        [Fact]
        public void WhenInitializeModuleLoaderAndConfigurationFileDoesntExistThenExceptionIsThrown()
        {
            // ARRANGE
            var nugetClient = new NugetClient(new HttpClientFactory());
            var currentDirectory = Directory.GetCurrentDirectory();
            var parent = Directory.GetParent(currentDirectory);
            var options = new ModuleLoaderOptions
            {
                ModulePath = parent.FullName,
                NugetSources = new List<string>
                {
                    "nugetsource"
                }
            };
            var moduleLoader = new ModuleLoader(nugetClient, options);

            // ACT
            var exception = Assert.Throws<FileNotFoundException>(() => moduleLoader.Initialize());

            // ASSERT
            Assert.NotNull(exception);
        }

        #endregion

        #region RestorePackages

        [Fact]
        public async Task WhenRestorePackagesAndModuleLoaderIsNotInitializedThenExceptionIsThrown()
        {
            // ARRANGE
            var nugetClient = new NugetClient(new HttpClientFactory());
            var options = new ModuleLoaderOptions();
            var moduleLoader = new ModuleLoader(nugetClient, options);

            // ACT
            var ex = await Assert.ThrowsAsync<ModuleLoaderInternalException>(() => moduleLoader.RestorePackages());

            // ASSERT
            Assert.NotNull(ex);
        }

        #endregion
    }
}