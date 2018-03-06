using Microsoft.Extensions.DependencyInjection;
using Moq;
using SimpleIdentityServer.Client;
using SimpleIdentityServer.Client.Configuration;
using SimpleIdentityServer.Client.Factories;
using SimpleIdentityServer.Client.Operations;
using SimpleIdentityServer.Client.Permission;
using SimpleIdentityServer.Client.Policy;
using SimpleIdentityServer.Client.ResourceSet;
using SimpleIdentityServer.Client.Selectors;
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Core.Jwt.Signature;
using SimpleIdentityServer.Uma.Common.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Uma.Host.Tests
{
    public class TokenFixture : IClassFixture<TestUmaServerFixture>
    {
        private const string baseUrl = "http://localhost:5000";
        private Mock<IHttpClientFactory> _httpClientFactoryStub;
        private Mock<SimpleIdentityServer.Uma.Client.Factory.IHttpClientFactory> _umaHttpClientFactoryStub;
        private IJwsGenerator _jwsGenerator;
        private IClientAuthSelector _clientAuthSelector;
        private IResourceSetClient _resourceSetClient;
        private IPermissionClient _permissionClient;
        private IPolicyClient _policyClient;
        private readonly TestUmaServerFixture _server;

        public TokenFixture(TestUmaServerFixture server)
        {
            _server = server;
        }

        [Fact]
        public async Task When_Using_ClientCredentials_Grant_Type_Then_AccessToken_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var result = await _clientAuthSelector.UseClientSecretPostAuth("resource_server", "resource_server")
                .UseClientCredentials("uma_protection", "uma_authorization")
                .ResolveAsync(baseUrl + "/.well-known/uma2-configuration");

            // ASSERTS
            Assert.NotNull(result);
            Assert.NotEmpty(result.AccessToken);
        }

        [Fact]
        public async Task When_Using_TicketId_Grant_Type_Then_AccessToken_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);
            _umaHttpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            var jwsPayload = new JwsPayload();
            jwsPayload.Add("iss", "http://server.example.com");
            jwsPayload.Add("sub", "248289761001");
            jwsPayload.Add("aud", "s6BhdRkqt3");
            jwsPayload.Add("nonce", "n-0S6_WzA2Mj");
            jwsPayload.Add("exp", "1311281970");
            jwsPayload.Add("iat", "1311280970");
            var jwt = _jwsGenerator.Generate(jwsPayload, JwsAlg.RS256, _server.SharedCtx.SignatureKey);

            // ACT
            var result = await _clientAuthSelector.UseClientSecretPostAuth("resource_server", "resource_server") // Get PAT.
                .UseClientCredentials("uma_protection", "uma_authorization")
                .ResolveAsync(baseUrl + "/.well-known/uma2-configuration");
            var resource = await _resourceSetClient.AddByResolution(new PostResourceSet // Add ressource.
            {
                Name = "name",
                Scopes = new List<string>
                {
                    "read",
                    "write",
                    "execute"
                }
            },
            baseUrl + "/.well-known/uma2-configuration", result.AccessToken);
            var addPolicy = await _policyClient.AddByResolution(new PostPolicy // Add an authorization policy.
            {
                Rules = new List<PostPolicyRule>
                {
                    new PostPolicyRule
                    {
                        IsResourceOwnerConsentNeeded = false,
                        Scopes = new List<string>
                        {
                            "read"
                        },
                        ClientIdsAllowed = new List<string>
                        {
                            "resource_server"
                        },
                        Claims = new List<PostClaim>
                        {
                            new PostClaim { Type = "sub", Value = "248289761001" }
                        }
                    }
                },
                ResourceSetIds = new List<string>
                {
                    resource.Id
                }
            }, baseUrl + "/.well-known/uma2-configuration", result.AccessToken);
            var ticket = await _permissionClient.AddByResolution(new PostPermission // Add permission & retrieve a ticket id.
            {
                ResourceSetId = resource.Id,
                Scopes = new List<string>
                {
                    "read"
                }
            }, baseUrl + "/.well-known/uma2-configuration", "header");
            var token = await _clientAuthSelector.UseClientSecretPostAuth("resource_server", "resource_server") // Try to get the access token via "ticket_id" grant-type.
                .UseTicketId(ticket.TicketId, jwt)
                .ResolveAsync(baseUrl + "/.well-known/uma2-configuration");

            // ASSERTS.
            Assert.NotNull(token);            
        }

        private void InitializeFakeObjects()
        {
            var services = new ServiceCollection();
            services.AddSimpleIdentityServerJwt();
            var provider = services.BuildServiceProvider();
            _jwsGenerator = provider.GetService<IJwsGenerator>();
            _httpClientFactoryStub = new Mock<IHttpClientFactory>();
            _umaHttpClientFactoryStub = new Mock<SimpleIdentityServer.Uma.Client.Factory.IHttpClientFactory>();
            var postTokenOperation = new PostTokenOperation(_httpClientFactoryStub.Object);
            var getDiscoveryOperation = new GetDiscoveryOperation(_httpClientFactoryStub.Object);
            var introspectionOperation = new IntrospectOperation(_httpClientFactoryStub.Object);
            var revokeTokenOperation = new RevokeTokenOperation(_httpClientFactoryStub.Object);
            _clientAuthSelector = new ClientAuthSelector(
                new TokenClientFactory(postTokenOperation, getDiscoveryOperation),
                new IntrospectClientFactory(introspectionOperation, getDiscoveryOperation),
                new RevokeTokenClientFactory(revokeTokenOperation, getDiscoveryOperation));
            _resourceSetClient = new ResourceSetClient(new AddResourceSetOperation(_umaHttpClientFactoryStub.Object),
                new DeleteResourceSetOperation(_umaHttpClientFactoryStub.Object),
                new GetResourcesOperation(_umaHttpClientFactoryStub.Object),
                new GetResourceOperation(_umaHttpClientFactoryStub.Object),
                new UpdateResourceOperation(_umaHttpClientFactoryStub.Object),
                new GetConfigurationOperation(_umaHttpClientFactoryStub.Object));
            _permissionClient = new PermissionClient(
                new AddPermissionsOperation(_umaHttpClientFactoryStub.Object),
                new GetConfigurationOperation(_umaHttpClientFactoryStub.Object));
            _policyClient = new PolicyClient(new AddPolicyOperation(_umaHttpClientFactoryStub.Object),
                new GetPolicyOperation(_umaHttpClientFactoryStub.Object),
                new DeletePolicyOperation(_umaHttpClientFactoryStub.Object),
                new GetPoliciesOperation(_umaHttpClientFactoryStub.Object),
                new AddResourceToPolicyOperation(_umaHttpClientFactoryStub.Object),
                new DeleteResourceFromPolicyOperation(_umaHttpClientFactoryStub.Object),
                new UpdatePolicyOperation(_umaHttpClientFactoryStub.Object),
                new GetConfigurationOperation(_umaHttpClientFactoryStub.Object));
        }
    }
}
