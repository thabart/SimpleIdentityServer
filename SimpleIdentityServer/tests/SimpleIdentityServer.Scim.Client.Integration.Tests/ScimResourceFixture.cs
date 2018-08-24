using SimpleIdentityServer.Client;
using SimpleIdentityServer.Scim.Mapping.Ad.Client;
using SimpleIdentityServer.Scim.Mapping.Ad.Common.DTOs.Requests;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Scim.Client.Integration.Tests
{
    public class ScimResourceFixture
    {
        [Fact]
        public async Task When_Add_Mapping_Then_Attributes_Are_Correctly_Returned()
        {
            const string scimBaseUrl = "http://localhost:60001";
            var identityServerClientFactory = new IdentityServerClientFactory();
            var adMappingClientFactory = new AdMappingClientFactory();
            var scimClientFactory = new ScimClientFactory();
            var adConfigurationClient = adMappingClientFactory.GetAdConfigurationClient();
            var adMappingClient = adMappingClientFactory.GetAdMappingClient();
            var tokenResponse = await identityServerClientFactory.CreateAuthSelector()
                .UseClientSecretPostAuth("ResourceServer", "LW46am54neU/[=Su")
                .UseClientCredentials("scim_manage", "scim_read")
                .ResolveAsync("http://localhost:60004/.well-known/uma2-configuration");
            var r = await adConfigurationClient.UpdateConfiguration(new UpdateAdConfigurationRequest
            {
                IpAdr = "127.0.0.1",
                Port = 10389,
                UserFilter = "(uid=${externalId})",
                DistinguishedName = "ou=system",
                Username = "uid=admin,ou=system",
                Password = "secret"
            }, scimBaseUrl, tokenResponse.Content.AccessToken);
            await adMappingClient.AddMapping(new AddMappingRequest
            {
                AdPropertyName = "cn",
                AttributeId = "314b9eb0-7b2a-46eb-8d7f-5b3d58421a99"
            }, scimBaseUrl, tokenResponse.Content.AccessToken);
            var user = await scimClientFactory.GetUserClient().GetUser(scimBaseUrl, "7d79392f-8a02-494c-949e-723a4db8ed16", tokenResponse.Content.AccessToken);

            string s = "";
        }
    }
}
