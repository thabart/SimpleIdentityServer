#region copyright
// Copyright 2016 Habart Thierry
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using Moq;
using SimpleIdentityServer.Client;
using SimpleIdentityServer.Client.Builders;
using SimpleIdentityServer.Client.Factories;
using SimpleIdentityServer.Client.Operations;
using SimpleIdentityServer.Client.Selectors;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Host.Tests
{
    public class IntrospectClientFixture : IClassFixture<TestScimServerFixture>
    {
        private const string baseUrl = "http://localhost:5000";
        private readonly TestScimServerFixture _server;
        private Mock<IHttpClientFactory> _httpClientFactoryStub;
        private IClientAuthSelector _clientAuthSelector;

        public IntrospectClientFixture(TestScimServerFixture server)
        {
            _server = server;
        }

        [Fact]
        public async Task When_Introspecting_AccessToken_Then_Information_Are_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var result = await _clientAuthSelector.UseClientSecretPostAuth("client", "client")
                .UsePassword("administrator", "password", "scim")
                .ResolveAsync(baseUrl + "/.well-known/openid-configuration");
            var introspection = await _clientAuthSelector.UseClientSecretPostAuth("client", "client")
                .Introspect(result.AccessToken, TokenType.AccessToken)
                .ResolveAsync(baseUrl + "/.well-known/openid-configuration");

            // ASSERT
            Assert.NotNull(introspection);
            Assert.NotNull(introspection.Scope);
            Assert.True(introspection.Scope.Count() == 1 && introspection.Scope.First() == "scim");
        }

        [Fact]
        public async Task When_Introspecting_IdToken_Then_Information_Are_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var result = await _clientAuthSelector.UseClientSecretPostAuth("client", "client")
                .UsePassword("administrator", "password", "scim")
                .ResolveAsync(baseUrl + "/.well-known/openid-configuration");
            var introspection = await _clientAuthSelector.UseClientSecretPostAuth("client", "client")
                .Introspect(result.IdToken, TokenType.AccessToken)
                .ResolveAsync(baseUrl + "/.well-known/openid-configuration");

            // ASSERT
            Assert.NotNull(introspection);
            Assert.NotNull(introspection.Scope);
            Assert.True(introspection.Scope.Count() == 1 && introspection.Scope.First() == "scim");
        }

        private void InitializeFakeObjects()
        {
            _httpClientFactoryStub = new Mock<IHttpClientFactory>();
            var requestBuilder = new RequestBuilder();
            var postTokenOperation = new PostTokenOperation(_httpClientFactoryStub.Object);
            var getDiscoveryOperation = new GetDiscoveryOperation(_httpClientFactoryStub.Object);
            var introspectionOperation = new IntrospectOperation(_httpClientFactoryStub.Object);
            var revokeTokenOperation = new RevokeTokenOperation(_httpClientFactoryStub.Object);
            _clientAuthSelector = new ClientAuthSelector(
                new TokenClientFactory(postTokenOperation, getDiscoveryOperation), 
                new IntrospectClientFactory(introspectionOperation, getDiscoveryOperation),
                new RevokeTokenClientFactory(revokeTokenOperation, getDiscoveryOperation));
        }
    }
}
