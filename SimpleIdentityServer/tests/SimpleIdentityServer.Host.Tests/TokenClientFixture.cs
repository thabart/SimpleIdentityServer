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

using Microsoft.Extensions.DependencyInjection;
using Moq;
using SimpleIdentityServer.Client;
using SimpleIdentityServer.Client.Builders;
using SimpleIdentityServer.Client.Factories;
using SimpleIdentityServer.Client.Operations;
using SimpleIdentityServer.Client.Selectors;
using SimpleIdentityServer.Core.Extensions;
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Core.Jwt.Encrypt;
using SimpleIdentityServer.Core.Jwt.Signature;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Host.Tests
{
    public class TokenClientFixture : IClassFixture<TestScimServerFixture>
    {
        private const string baseUrl = "http://localhost:5000";
        private readonly TestScimServerFixture _server;
        private Mock<IHttpClientFactory> _httpClientFactoryStub;
        private IClientAuthSelector _clientAuthSelector;
        private IUserInfoClient _userInfoClient;
        private IJwsGenerator _jwsGenerator;
        private IJweGenerator _jweGenerator;

        public TokenClientFixture(TestScimServerFixture server)
        {
            _server = server;
        }

        #region GrantTypes

        [Fact]
        public async Task When_Using_Password_Grant_Type_Then_Access_Token_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var result = await _clientAuthSelector.UseClientSecretPostAuth("client", "client")
                .UsePassword("administrator", "password", "scim")
                .ResolveAsync(baseUrl + "/.well-known/openid-configuration");
            // var claims = await _userInfoClient.Resolve(baseUrl + "/.well-known/openid-configuration", result.AccessToken);

            // ASSERTS
            Assert.NotNull(result);
            Assert.NotEmpty(result.AccessToken);
            /*
            Assert.NotNull(claims);
            Assert.True(claims[Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject].ToString() == "administrator");
            Assert.True(claims[Core.Jwt.Constants.StandardResourceOwnerClaimNames.ScimId].ToString() == "id");
            Assert.True(claims[Core.Jwt.Constants.StandardResourceOwnerClaimNames.ScimLocation].ToString() == "http://localhost:5555/Users/id");
            */
        }

        [Fact]
        public async Task When_Using_Client_Certificate_Then_AccessToken_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);



        }

        [Fact]
        public async Task When_Using_RefreshToken_GrantType_Then_New_One_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var result = await _clientAuthSelector.UseClientSecretPostAuth("client", "client")
                .UsePassword("administrator", "password", "scim")
                .ResolveAsync(baseUrl + "/.well-known/openid-configuration");
            var refreshToken = await _clientAuthSelector.UseNoAuthentication()
                .UseRefreshToken(result.RefreshToken)
                .ResolveAsync(baseUrl + "/.well-known/openid-configuration");

            // ASSERTS
            Assert.NotNull(result);
            Assert.NotEmpty(result.AccessToken);
        }

        #endregion

        #region Client authentications

        [Fact]
        public async Task When_Using_ClientSecretPostAuthentication_Then_AccessToken_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);


            // ACT
            var token = await _clientAuthSelector.UseClientSecretBasicAuth("basic_client", "basic_client")
                .UseClientCredentials("api1")
                .ResolveAsync(baseUrl + "/.well-known/openid-configuration");

            // ASSERTS
            Assert.NotNull(token);
            Assert.NotEmpty(token.AccessToken);
        }

        [Fact]
        public async Task When_Using_BaseAuthentication_Then_AccessToken_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);


            // ACT
            var firstToken = await _clientAuthSelector.UseClientSecretBasicAuth("basic_client", "basic_client")
                .UseClientCredentials("api1")
                .ResolveAsync(baseUrl + "/.well-known/openid-configuration");

            // ASSERTS
            Assert.NotNull(firstToken);
            Assert.NotEmpty(firstToken.AccessToken);
        }

        [Fact]
        public async Task When_Using_ClientSecretJwtAuthentication_Then_AccessToken_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);
            var payload = new JwsPayload
            {
                {
                    Core.Jwt.Constants.StandardClaimNames.Issuer, "jwt_client"
                },
                {
                    Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject, "jwt_client"
                },
                {
                    Core.Jwt.Constants.StandardClaimNames.Audiences, new []
                    {
                        "http://localhost:5000"
                    }
                },
                {
                    Core.Jwt.Constants.StandardClaimNames.ExpirationTime, DateTime.UtcNow.AddHours(1).ConvertToUnixTimestamp()
                }
            };
            var jws = _jwsGenerator.Generate(payload, JwsAlg.RS256, _server.SharedCtx.SignatureKey);
            var jwe = _jweGenerator.GenerateJweByUsingSymmetricPassword(jws, JweAlg.RSA1_5, JweEnc.A128CBC_HS256, _server.SharedCtx.EncryptionKey, "jwt_client");

            // ACT
            var token = await _clientAuthSelector.UseClientSecretJwtAuth(jwe, "jwt_client")
                .UseClientCredentials("api1")
                .ResolveAsync(baseUrl + "/.well-known/openid-configuration");


            // ASSERTS
            Assert.NotNull(token);
            Assert.NotEmpty(token.AccessToken);
        }

        [Fact]
        public async Task When_Using_PrivateKeyJwtAuthentication_Then_AccessToken_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);
            var payload = new JwsPayload
            {
                {
                    Core.Jwt.Constants.StandardClaimNames.Issuer, "private_key_client"
                },
                {
                    Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject, "private_key_client"
                },
                {
                    Core.Jwt.Constants.StandardClaimNames.Audiences, new []
                    {
                        "http://localhost:5000"
                    }
                },
                {
                    Core.Jwt.Constants.StandardClaimNames.ExpirationTime, DateTime.UtcNow.AddHours(1).ConvertToUnixTimestamp()
                }
            };
            var jws = _jwsGenerator.Generate(payload, JwsAlg.RS256, _server.SharedCtx.SignatureKey);

            // ACT
            var token = await _clientAuthSelector.UseClientPrivateKeyAuth(jws, "private_key_client")
                .UseClientCredentials("api1")
                .ResolveAsync(baseUrl + "/.well-known/openid-configuration");

            // ASSERTS
            Assert.NotNull(token);
            Assert.NotEmpty(token.AccessToken);
        }

        #endregion

        private void InitializeFakeObjects()
        {
            var services = new ServiceCollection();
            services.AddSimpleIdentityServerJwt();
            var provider = services.BuildServiceProvider();
            _jwsGenerator = (IJwsGenerator)provider.GetService(typeof(IJwsGenerator));
            _jweGenerator = (IJweGenerator)provider.GetService(typeof(IJweGenerator));
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
            var getUserInfoOperation = new GetUserInfoOperation(_httpClientFactoryStub.Object);
            _userInfoClient = new UserInfoClient(getUserInfoOperation, getDiscoveryOperation);
        }
    }
}
