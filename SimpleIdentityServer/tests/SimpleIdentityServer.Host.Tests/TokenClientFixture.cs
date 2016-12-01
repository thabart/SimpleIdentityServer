#region copyright
// Copyright 2015 Habart Thierry
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
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Host.Tests
{
    public class TokenClientFixture : IClassFixture<TestScimServerFixture>
    {
        private readonly TestScimServerFixture _server;
        private Mock<IHttpClientFactory> _httpClientFactoryStub;
        private IClientAuthSelector _clientAuthSelector;
        private IUserInfoClient _userInfoClient;

        public TokenClientFixture(TestScimServerFixture server)
        {
            _server = server;
        }

        [Fact]
        public async Task When_Requesting_Token_Then_No_Exception_Is_Thrown()
        {
            const string baseUrl = "http://localhost:5000";
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT : Get access token via password grant-type.
            var result = await _clientAuthSelector.UseClientSecretPostAuth("client", "client")
                .UsePassword("administrator", "password", "scim")
                .ResolveAsync(baseUrl + "/.well-known/openid-configuration");

            // ASSERTS
            Assert.NotNull(result);
            Assert.NotEmpty(result.AccessToken);

            // ACT : Get user information.
            var claims = await _userInfoClient.Resolve(baseUrl + "/.well-known/openid-configuration", result.AccessToken);

            // ASSERTS
            Assert.NotNull(claims);
            Assert.True(claims[Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject].ToString() == "administrator");
            Assert.True(claims[Core.Jwt.Constants.StandardResourceOwnerClaimNames.ScimId].ToString() == "id");
            Assert.True(claims[Core.Jwt.Constants.StandardResourceOwnerClaimNames.ScimLocation].ToString() == "http://localhost:5555/Users/id");
        }

        private void InitializeFakeObjects()
        {
            _httpClientFactoryStub = new Mock<IHttpClientFactory>();
            var tokenRequestBuilder = new TokenRequestBuilder();
            var postTokenOperation = new PostTokenOperation(_httpClientFactoryStub.Object);
            var getDiscoveryOperation = new GetDiscoveryOperation(_httpClientFactoryStub.Object);
            var tokenClient = new TokenClient(tokenRequestBuilder, postTokenOperation, getDiscoveryOperation);
            var tokenGrantTypeSelector = new TokenGrantTypeSelector(tokenRequestBuilder, tokenClient);
            _clientAuthSelector = new ClientAuthSelector(tokenRequestBuilder, tokenGrantTypeSelector);
            var getUserInfoOperation = new GetUserInfoOperation(_httpClientFactoryStub.Object);
            _userInfoClient = new UserInfoClient(getUserInfoOperation, getDiscoveryOperation);
        }
    }
}
