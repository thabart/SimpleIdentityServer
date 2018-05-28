using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Module.Loader.Integration.Tests
{
    public class ModuleLoaderFixture
    {
        [Fact]
        public async Task WhenRestorePackages()
        {
            var factory = new ModuleLoaderFactory();
            var moduleLoader = factory.BuidlerModuleLoader(new ModuleLoaderOptions
            {
                NugetSources = new List<string>
                {
                    @"c:\Projects\SimpleIdentityServer\SimpleIdentityServer\src\feed\",
                    "https://api.nuget.org/v3/index.json",
                    "https://www.myget.org/F/advance-ict/api/v3/index.json"
                },
                ModulePath = @"D:\Modules"
            });

            moduleLoader.Initialize();
            await moduleLoader.RestorePackages();
        }
    }
}
