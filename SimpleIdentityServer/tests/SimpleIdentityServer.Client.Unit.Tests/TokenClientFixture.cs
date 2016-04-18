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
using SimpleIdentityServer.Client.Builders;
using SimpleIdentityServer.Client.DTOs.Request;
using SimpleIdentityServer.Client.DTOs.Response;
using SimpleIdentityServer.Client.Errors;
using SimpleIdentityServer.Client.Operations;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Client.Unit.Tests
{
    public class TokenClientFixture
    {
        private Mock<ITokenRequestBuilder> _tokenRequestBuilderStub;

        private Mock<IPostTokenOperation> _postTokenOperationStub;

        private Mock<IGetDiscoveryOperation> _getDiscoveryOperationStub;

        private ITokenClient _tokenClient;

        #region Exceptions

        [Fact]
        public void When_Passing_Null_Parameters_Then_Exceptions_Are_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACTS & ASSERTS
            Assert.ThrowsAsync<ArgumentNullException>(() => _tokenClient.ExecuteAsync(string.Empty));
            Assert.ThrowsAsync<ArgumentNullException>(() => _tokenClient.ResolveAsync(string.Empty));
        }

        [Fact]
        public void When_Passing_Invalid_Url_To_ExecuteAsync_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            const string tokenUrl = "invalid_url";
            InitializeFakeObjects();

            // ACT & ASSERTS
            var exception = Assert.ThrowsAsync<ArgumentException>(() => _tokenClient.ExecuteAsync(tokenUrl));
            var content = exception.Result;
            Assert.NotNull(exception);
            Assert.True(content.Message == string.Format(ErrorDescriptions.TheUrlIsNotWellFormed, tokenUrl));
        }

        [Fact]
        public void When_Passing_Invalid_Url_To_ResolveAsync_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            const string tokenUrl = "invalid_url";
            InitializeFakeObjects();

            // ACT & ASSERTS
            var exception = Assert.ThrowsAsync<ArgumentException>(() => _tokenClient.ResolveAsync(tokenUrl));
            var content = exception.Result;
            Assert.NotNull(exception);
            Assert.True(content.Message == string.Format(ErrorDescriptions.TheUrlIsNotWellFormed, tokenUrl));
        }

        #endregion

        #region Happy paths

        [Fact]
        public async Task When_Getting_Token_Via_ExecuteAsync_Then_GrantedToken_Is_Returned()
        {
            // ARRANGE
            const string tokenUrl = "https://localhost/token";
            const string authorizationHeaderValue = "authorization";
            var tokenRequest = new TokenRequest
            {
                Scope = "scope"
            };
            InitializeFakeObjects();
            _tokenRequestBuilderStub.Setup(t => t.AuthorizationHeaderValue).Returns(authorizationHeaderValue);
            _tokenRequestBuilderStub.Setup(t => t.TokenRequest).Returns(tokenRequest);

            // ACT
            var result = await _tokenClient.ExecuteAsync(tokenUrl);

            // ASSERT
            _postTokenOperationStub.Verify(p => p.ExecuteAsync(tokenRequest,
                new Uri(tokenUrl),
                authorizationHeaderValue));
        }

        [Fact]
        public async Task When_Getting_Token_Via_ResolveAsync_Then_GrantedToken_Is_Returned()
        {
            // ARRANGE
            const string discoveryUrl = "https://localhost/.well-known/openid-configuration";
            const string tokenUrl = "https://localhost/token";
            const string authorizationHeaderValue = "authorization";
            var tokenRequest = new TokenRequest
            {
                Scope = "scope"
            };
            var discoveryInformation = new DiscoveryInformation
            {
                TokenEndPoint = tokenUrl
            };
            InitializeFakeObjects();
            _getDiscoveryOperationStub.Setup(g => g.ExecuteAsync(It.IsAny<Uri>())).ReturnsAsync(discoveryInformation);
            _tokenRequestBuilderStub.Setup(t => t.AuthorizationHeaderValue).Returns(authorizationHeaderValue);
            _tokenRequestBuilderStub.Setup(t => t.TokenRequest).Returns(tokenRequest);

            // ACT
            var result = await _tokenClient.ResolveAsync(discoveryUrl);

            // ASSERT
            _getDiscoveryOperationStub.Verify(g => g.ExecuteAsync(new Uri(discoveryUrl)));
            _postTokenOperationStub.Verify(p => p.ExecuteAsync(tokenRequest,
                new Uri(tokenUrl),
                authorizationHeaderValue));
        }

        #endregion

        private void InitializeFakeObjects()
        {
            _tokenRequestBuilderStub = new Mock<ITokenRequestBuilder>();
            _postTokenOperationStub = new Mock<IPostTokenOperation>();
            _getDiscoveryOperationStub = new Mock<IGetDiscoveryOperation>();
            _tokenClient = new TokenClient(
                _tokenRequestBuilderStub.Object,
                _postTokenOperationStub.Object,
                _getDiscoveryOperationStub.Object);
        }
    }
}
