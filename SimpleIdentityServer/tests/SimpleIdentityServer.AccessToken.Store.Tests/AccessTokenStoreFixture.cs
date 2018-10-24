using Moq;
using SimpleIdentityServer.Client;
using SimpleIdentityServer.Client.Results;
using SimpleIdentityServer.Client.Selectors;
using SimpleIdServer.Storage;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.AccessToken.Store.Tests
{
    public class AccessTokenStoreFixture
    {
        private Mock<IIdentityServerClientFactory> _identityServerClientFactoryStub;
        private Mock<IStorage> _storageStub;
        private AccessTokenStore _accessTokenStore;

        [Fact]
        public async Task When_Pass_Null_Parameters_Then_Exceptions_Are_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            await Assert.ThrowsAsync<ArgumentNullException>(() => _accessTokenStore.GetToken(null, null, null, null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => _accessTokenStore.GetToken("url", null, null, null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => _accessTokenStore.GetToken("url", "clientid", null, null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => _accessTokenStore.GetToken("url", "clientid", "clientsecret", null));
        }

        [Fact]
        public async Task When_Get_AccessToken_Then_NewOne_Is_Inserted_In_The_Cache()
        {
            // ARRANGE
            InitializeFakeObjects();
            var tokenClient = new Mock<ITokenClient>();
            tokenClient.Setup(t => t.ResolveAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(new GetTokenResult
                {
                    Content = new Core.Common.DTOs.Responses.GrantedTokenResponse
                    {
                        ExpiresIn = 3600
                    }
                }));
            var tokenGrantTypeSelector = new Mock<ITokenGrantTypeSelector>();
            tokenGrantTypeSelector.Setup(c => c.UseClientCredentials(It.IsAny<string[]>()))
                .Returns(tokenClient.Object);
            var clientAuthSelector = new Mock<IClientAuthSelector>();
            clientAuthSelector.Setup(c => c.UseClientSecretPostAuth(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(tokenGrantTypeSelector.Object);
            _identityServerClientFactoryStub.Setup(i => i.CreateAuthSelector()).Returns(clientAuthSelector.Object);

            // ACT
            var result = await _accessTokenStore.GetToken("url", "clientid", "clientsecret", new[] { "scope" }).ConfigureAwait(false);

            // ASSERT
            _storageStub.Verify(s => s.SetAsync("access_tokens", It.IsAny<object>()));
        }

        [Fact]
        public async Task When_Get_AccessToken_Then_AccessToken_Is_Returned_From_The_Cache()
        {
            // ARRANGE
            InitializeFakeObjects();
            var tokenClient = new Mock<ITokenClient>();
            _storageStub.Setup(s => s.TryGetValueAsync<List<StoredToken>>(It.IsAny<string>())).Returns(Task.FromResult(new List<StoredToken>
            {
                new StoredToken
                {
                    Url = "http://localhost",
                    Scopes = new []
                    {
                        "scope"
                    },
                    ExpirationDateTime = DateTime.UtcNow.AddSeconds(300),
                    GrantedToken = new Core.Common.DTOs.Responses.GrantedTokenResponse()
                }
            }));


            // ACT
            var result = await _accessTokenStore.GetToken("http://localhost", "clientid", "clientsecret", new[] { "scope" }).ConfigureAwait(false);

            // ASSERT
            Assert.NotNull(result);
        }

        private void InitializeFakeObjects()
        {
            _identityServerClientFactoryStub = new Mock<IIdentityServerClientFactory>();
            _storageStub = new Mock<IStorage>();
            _accessTokenStore = new AccessTokenStore(_storageStub.Object, _identityServerClientFactoryStub.Object);
        }
    }
}
