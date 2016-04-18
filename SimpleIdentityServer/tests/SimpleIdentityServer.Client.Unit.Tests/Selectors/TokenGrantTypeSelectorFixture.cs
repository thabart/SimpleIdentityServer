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
using SimpleIdentityServer.Client.Selectors;
using System;
using System.Collections.Generic;
using Xunit;

namespace SimpleIdentityServer.Client.Unit.Tests.Selectors
{
    public class TokenGrantTypeSelectorFixture
    {
        private Mock<ITokenRequestBuilder> _tokenRequestBuilderStub;

        private Mock<ITokenClient> _tokenClientStub;

        private ITokenGrantTypeSelector _tokenGrantTypeSelector;

        #region Exceptions

        [Fact]
        public void When_Passing_Empty_Parameters_Then_Exceptions_Are_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACTS & ASSERTS
            Assert.Throws<ArgumentNullException>(() => _tokenGrantTypeSelector.UseClientCredentials());
            Assert.Throws<ArgumentNullException>(() => _tokenGrantTypeSelector.UseClientCredentials(new List<string>()));
        }

        #endregion

        #region Happy paths

        [Fact]
        public void When_Using_ClientCredentialsGrantType_Then_TokenClient_Is_Returned()
        {
            // ARRANGE
            const string firstScope = "scope";
            const string secondScope = "scope_2";
            var tokenRequest = new TokenRequest();
            InitializeFakeObjects();
            _tokenRequestBuilderStub.Setup(t => t.TokenRequest).Returns(tokenRequest);

            // ACT
            var result = _tokenGrantTypeSelector.UseClientCredentials(firstScope, secondScope);

            // ASSERTS
            Assert.NotNull(result);
            Assert.True(tokenRequest.Scope == firstScope + " " + secondScope);
            Assert.True(tokenRequest.GrantType == GrantTypeRequest.client_credentials);
        }

        [Fact]
        public void When_Using_ClientCredentialsGrantType_And_Using_List_Of_Scopes_Then_TokenClient_Is_Returned()
        {
            // ARRANGE
            const string firstScope = "scope";
            const string secondScope = "scope_2";
            var tokenRequest = new TokenRequest();
            InitializeFakeObjects();
            _tokenRequestBuilderStub.Setup(t => t.TokenRequest).Returns(tokenRequest);

            // ACT
            var result = _tokenGrantTypeSelector.UseClientCredentials(new List<string> { firstScope, secondScope });

            // ASSERTS
            Assert.NotNull(result);
            Assert.True(tokenRequest.Scope == firstScope + " " + secondScope);
            Assert.True(tokenRequest.GrantType == GrantTypeRequest.client_credentials);
        }

        #endregion

        private void InitializeFakeObjects()
        {
            _tokenRequestBuilderStub = new Mock<ITokenRequestBuilder>();
            _tokenClientStub = new Mock<ITokenClient>();
            _tokenGrantTypeSelector = new TokenGrantTypeSelector(
                _tokenRequestBuilderStub.Object,
                _tokenClientStub.Object);
        }
    }
}
