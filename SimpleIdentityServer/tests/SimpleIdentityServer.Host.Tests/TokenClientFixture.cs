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
using System.Security.Cryptography;
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
        private IJwsGenerator _jwsGenerator;
        private IJweGenerator _jweGenerator;

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

            // ACT : Get access token via password grant-type & user post authentication.
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

            // ACT : Get access token valid for the scope "api1" & use basic authentication.
            var firstToken = await _clientAuthSelector.UseClientSecretBasicAuth("api_client", "api_client")
                .UseClientCredentials("api1")
                .ResolveAsync(baseUrl + "/.well-known/openid-configuration");

            // ASSERTS
            Assert.NotNull(firstToken);
            Assert.NotEmpty(firstToken.AccessToken);

            // ACT : Get access token valid for the scope "api1" & use client_secret_jwt authentication.
            var clientPayLoad = new JwsPayload
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
            var jws = _jwsGenerator.Generate(clientPayLoad, JwsAlg.RS256, _server.SharedCtx.SignatureKey);
            var jwe = _jweGenerator.GenerateJweByUsingSymmetricPassword(jws, JweAlg.RSA1_5, JweEnc.A128CBC_HS256, _server.SharedCtx.EncryptionKey, "jwt_client");
            var secondToken = await _clientAuthSelector.UseClientSecretJwtAuth(jwe, "jwt_client")
                .UseClientCredentials("api1")
                .ResolveAsync(baseUrl + "/.well-known/openid-configuration");

            // ASSERTS
            Assert.NotNull(secondToken);
            Assert.NotEmpty(secondToken.AccessToken);

            // ACT : Test refresh token.
            var thirdToken = await _clientAuthSelector.UseNoAuthentication()
                .UseRefreshToken(secondToken.RefreshToken)
                .ResolveAsync(baseUrl + "/.well-known/openid-configuration");

            // ASSERTS
            Assert.NotNull(thirdToken);

            // ACT : Test private key jwt
            var secondClientPayLoad = new JwsPayload
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
            var secondJws = _jwsGenerator.Generate(secondClientPayLoad, JwsAlg.RS256, _server.SharedCtx.SignatureKey);
            var fourthToken = await _clientAuthSelector.UseClientPrivateKeyAuth(secondJws, "private_key_client")
                .UseClientCredentials("api1")
                .ResolveAsync(baseUrl + "/.well-known/openid-configuration");

            // ASSERTS
            Assert.NotNull(fourthToken);
        }

        private void InitializeFakeObjects()
        {
            var services = new ServiceCollection();
            services.AddSimpleIdentityServerJwt();
            var provider = services.BuildServiceProvider();
            _jwsGenerator = (IJwsGenerator)provider.GetService(typeof(IJwsGenerator));
            _jweGenerator = (IJweGenerator)provider.GetService(typeof(IJweGenerator));
            _httpClientFactoryStub = new Mock<IHttpClientFactory>();
            var tokenRequestBuilder = new TokenRequestBuilder();
            var postTokenOperation = new PostTokenOperation(_httpClientFactoryStub.Object);
            var getDiscoveryOperation = new GetDiscoveryOperation(_httpClientFactoryStub.Object);
            var tokenClient = new TokenClient(tokenRequestBuilder, postTokenOperation, getDiscoveryOperation);
            var tokenGrantTypeSelector = new TokenGrantTypeSelector(tokenRequestBuilder, tokenClient);
            _clientAuthSelector = new ClientAuthSelector(new TokenClientFactory(postTokenOperation, getDiscoveryOperation));
            var getUserInfoOperation = new GetUserInfoOperation(_httpClientFactoryStub.Object);
            _userInfoClient = new UserInfoClient(getUserInfoOperation, getDiscoveryOperation);
        }
    }
}
