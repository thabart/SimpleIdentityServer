using SimpleIdentityServer.Module.Loader.Factories;
using SimpleIdentityServer.Module.Loader.Nuget;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Module.Loader.Tests
{
    public class NugetClientFixture
    {
        [Fact]
        public async Task WhenPassNullParameterToGetConfigurationThenExceptionIsThrown()
        {
            // ARRANGE
            var nugetClient = new NugetClient(new HttpClientFactory());

            // ASSERT
           await  Assert.ThrowsAsync<ArgumentNullException>(() => nugetClient.GetConfiguration(null));
        }

        [Fact]
        public async Task WhenPassNullParameterToGetNugetFlatContainerThenExceptionIsThrown()
        {
            // ARRANGE
            var nugetClient = new NugetClient(new HttpClientFactory());

            // ASSERT
            await Assert.ThrowsAsync<ArgumentNullException>(() => nugetClient.GetNugetFlatContainer(null, null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => nugetClient.GetNugetFlatContainer("url", null));
        }

        [Fact]
        public async Task WhenPassNullParameterToDownloadNugetPackageThenExceptionIsThrown()
        {
            // ARRANGE
            var nugetClient = new NugetClient(new HttpClientFactory());

            // ASSERT
            await Assert.ThrowsAsync<ArgumentNullException>(() => nugetClient.DownloadNugetPackage(null, null, null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => nugetClient.DownloadNugetPackage("url", null, null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => nugetClient.DownloadNugetPackage("url", "packageName", null));
        }
    }
}
