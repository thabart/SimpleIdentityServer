using SimpleIdentityServer.Client;
using SimpleIdentityServer.Scim.Mapping.Ad.Client;
using SimpleIdentityServer.Scim.Mapping.Ad.Common.DTOs.Requests;
using System.Collections.Generic;
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
                Schemas = new List<UpdateAdConfigurationSchemaRequest>
                {
                    new UpdateAdConfigurationSchemaRequest
                    {
                        Filter = "(uid=${externalId})",
                        FilterClass = "(objectClass=person)",
                        SchemaId = "urn:ietf:params:scim:schemas:core:2.0:User"
                    }
                },
                DistinguishedName = "ou=system",
                Username = "uid=admin,ou=system",
                Password = "secret",
                IsEnabled = true
            }, scimBaseUrl, tokenResponse.Content.AccessToken);
            await adMappingClient.AddMapping(new AddMappingRequest
            {
                AdPropertyName = "cn",
                AttributeId = "8c5f01ca-cd5a-4a87-b503-9c9977074947",
                SchemaId = "urn:ietf:params:scim:schemas:core:2.0:User"
            }, scimBaseUrl, tokenResponse.Content.AccessToken);
            var user = await scimClientFactory.GetUserClient().GetUser(scimBaseUrl, "7d79392f-8a02-494c-949e-723a4db8ed16", tokenResponse.Content.AccessToken);

            string s = "";
        }

        [Fact]
        public async Task When_Update_Configuration_And_Get_Properties_Then_List_Is_Returned()
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
                IsEnabled = true,
                Schemas = new List<UpdateAdConfigurationSchemaRequest>
                {
                    new UpdateAdConfigurationSchemaRequest
                    {
                        Filter = "(uid=${externalId})",
                        FilterClass = "(objectClass=person)",
                        SchemaId = "urn:ietf:params:scim:schemas:core:2.0:User"
                    }
                },
                DistinguishedName = "ou=system",
                Username = "uid=admin,ou=system",
                Password = "secret"
            }, scimBaseUrl, tokenResponse.Content.AccessToken);
            var result = await adMappingClient.GetAllProperties("urn:ietf:params:scim:schemas:core:2.0:User", scimBaseUrl, tokenResponse.Content.AccessToken);
            string s = "";
        }
    }
}