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
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Core.Jwt.Converter;
using SimpleIdentityServer.Core.Jwt.Signature;
using SimpleIdentityServer.Uma.Core.JwtToken;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Uma.Core.UnitTests.JwtToken
{
    public class JwtTokenParserFixture
    {
        private Mock<IJwsParser> _jwsParserStub;

        private Mock<IIdentityServerClientFactory> _identityServerClientFactoryStub;

        private Mock<IParametersProvider> _parametersProviderStub;

        private Mock<IJsonWebKeyConverter> _jsonWebKeyConverterStub;

        private IJwtTokenParser _jwtTokenParser;

        #region Exceptions

        [Fact]
        public void When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            Assert.ThrowsAsync<ArgumentNullException>(() => _jwtTokenParser.UnSign(null));
            Assert.ThrowsAsync<ArgumentNullException>(() => _jwtTokenParser.UnSign(string.Empty));
        }

        [Fact]
        public void When_Header_Cannot_Be_Extracted_Then_Null_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _jwsParserStub.Setup(j => j.GetHeader(It.IsAny<string>()))
                .Returns(() => null);

            // ACT
            var result = _jwtTokenParser.UnSign("invalid_token").Result;

            // ASSERT
            Assert.Null(result);
        }

        [Fact]
        public void When_No_JsonWebKeys_Can_Be_Extracted_Then_Null_Is_Returned()
        {
            // ARRANGE
            var header = new JwsProtectedHeader
            {
                Kid = "invalid"
            };
            InitializeFakeObjects();
            _jwsParserStub.Setup(j => j.GetHeader(It.IsAny<string>()))
                .Returns(header);
            _parametersProviderStub.Setup(p => p.GetOpenIdConfigurationUrl())
                .Returns("configuration");
            var jwksClientStub = new Mock<IJwksClient>();
            jwksClientStub.Setup(j => j.ResolveAsync(It.IsAny<string>()))
                .Returns(Task.FromResult<JsonWebKeySet>(null));
            _identityServerClientFactoryStub.Setup(i => i.CreateJwksClient()).Returns(jwksClientStub.Object);
            _jsonWebKeyConverterStub.Setup(j => j.ExtractSerializedKeys(It.IsAny<JsonWebKeySet>()))
                .Returns(() => null);

            // ACT
            var result = _jwtTokenParser.UnSign("token").Result;

            // ASSERT
            Assert.Null(result);
        }

        #endregion

        #region Happy paths
        
        [Fact]
        public async Task When_There_Is_No_Algorithm_Then_JwsPayload_Is_Returned()
        {
            // ARRANGE
            var header = new JwsProtectedHeader
            {
                Kid = "kid",
                Alg = SimpleIdentityServer.Core.Jwt.Constants.JwsAlgNames.NONE
            };
            var jsonWebKeys = new List<JsonWebKey>
            {
                new JsonWebKey
                {
                    Kid = "kid"
                }
            };
            InitializeFakeObjects();
            _jwsParserStub.Setup(j => j.GetHeader(It.IsAny<string>()))
                .Returns(header);
            _parametersProviderStub.Setup(p => p.GetOpenIdConfigurationUrl())
                .Returns("configuration");
            var jwksClientStub = new Mock<IJwksClient>();
            jwksClientStub.Setup(j => j.ResolveAsync(It.IsAny<string>()))
                .Returns(Task.FromResult<JsonWebKeySet>(null));
            _identityServerClientFactoryStub.Setup(i => i.CreateJwksClient()).Returns(jwksClientStub.Object);
            _jsonWebKeyConverterStub.Setup(j => j.ExtractSerializedKeys(It.IsAny<JsonWebKeySet>()))
                .Returns(jsonWebKeys);

            // ACT
            await _jwtTokenParser.UnSign("token");

            // ASSERT
            _jwsParserStub.Verify(j => j.GetPayload(It.IsAny<string>()));
        }

        [Fact]
        public async Task When_Unsign_JwsToken_Then_Signature_Is_Checked()
        {
            // ARRANGE
            var header = new JwsProtectedHeader
            {
                Kid = "kid",
                Alg = SimpleIdentityServer.Core.Jwt.Constants.JwsAlgNames.RS256
            };
            var jsonWebKeys = new List<JsonWebKey>
            {
                new JsonWebKey
                {
                    Kid = "kid"
                }
            };
            InitializeFakeObjects();
            _jwsParserStub.Setup(j => j.GetHeader(It.IsAny<string>()))
                .Returns(header);
            _parametersProviderStub.Setup(p => p.GetOpenIdConfigurationUrl())
                .Returns("configuration");
            var jwksClientStub = new Mock<IJwksClient>();
            jwksClientStub.Setup(j => j.ResolveAsync(It.IsAny<string>()))
                .Returns(Task.FromResult<JsonWebKeySet>(null));
            _identityServerClientFactoryStub.Setup(i => i.CreateJwksClient()).Returns(jwksClientStub.Object);
            _jsonWebKeyConverterStub.Setup(j => j.ExtractSerializedKeys(It.IsAny<JsonWebKeySet>()))
                .Returns(jsonWebKeys);

            // ACT
            await _jwtTokenParser.UnSign("token");

            // ASSERT
            _jwsParserStub.Verify(j => j.ValidateSignature(It.IsAny<string>(), It.IsAny<JsonWebKey>()));
        }

        #endregion

        #region Private methods

        private void InitializeFakeObjects()
        {
            _jwsParserStub = new Mock<IJwsParser>();
            _identityServerClientFactoryStub = new Mock<IIdentityServerClientFactory>();
            _parametersProviderStub = new Mock<IParametersProvider>();
            _jsonWebKeyConverterStub = new Mock<IJsonWebKeyConverter>();
            _jwtTokenParser = new JwtTokenParser(
                _jwsParserStub.Object,
                _identityServerClientFactoryStub.Object,
                _parametersProviderStub.Object,
                _jsonWebKeyConverterStub.Object);
        }

        #endregion
    }
}
