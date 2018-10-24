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
using SimpleIdentityServer.Core.Common;
using SimpleIdentityServer.Core.Common.Repositories;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.JwtToken;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.Helpers
{
    public sealed class ClientHelperFixture
    {
        private Mock<IClientRepository> _clientRepositoryStub;
        private Mock<IJwtGenerator> _jwtGeneratorStub;
        private Mock<IJwtParser> _jwtParserStub;
        private IClientHelper _clientHelper;

        [Fact]
        public async Task When_Passing_Null_Parameters_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERTS
            await Assert.ThrowsAsync<ArgumentNullException>(() => _clientHelper.GenerateIdTokenAsync(string.Empty, null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => _clientHelper.GenerateIdTokenAsync("client_id", null));
        }

        [Fact]
        public async Task When_Signed_Response_Alg_Is_Not_Passed_Then_RS256_Is_Used()
        {
            // ARRANGE
            InitializeFakeObjects();
            var client = new Core.Common.Models.Client();
            _clientRepositoryStub.Setup(c => c.GetClientByIdAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(client));

            // ACT
            await _clientHelper.GenerateIdTokenAsync("client_id", new JwsPayload());

            // ASSERT
            _jwtGeneratorStub.Verify(j => j.SignAsync(It.IsAny<JwsPayload>(), JwsAlg.RS256));
        }

        [Fact]
        public async Task When_Signed_Response_And_EncryptResponseAlg_Are_Passed_Then_EncryptResponseEnc_A128CBC_HS256_Is_Used()
        {
            // ARRANGE
            InitializeFakeObjects();
            var client = new Core.Common.Models.Client
            {
                IdTokenSignedResponseAlg = Jwt.Constants.JwsAlgNames.RS256,
                IdTokenEncryptedResponseAlg = Jwt.Constants.JweAlgNames.RSA1_5
            };
            _clientRepositoryStub.Setup(c => c.GetClientByIdAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(client));

            // ACT
            await _clientHelper.GenerateIdTokenAsync("client_id", new JwsPayload());

            // ASSERT
            _jwtGeneratorStub.Verify(j => j.SignAsync(It.IsAny<JwsPayload>(), JwsAlg.RS256));
            _jwtGeneratorStub.Verify(j => j.EncryptAsync(It.IsAny<string>(), JweAlg.RSA1_5, JweEnc.A128CBC_HS256));
        }
        
        [Fact]
        public async Task When_Sign_And_Encrypt_JwsPayload_Then_Functions_Are_Called()
        {
            // ARRANGE
            InitializeFakeObjects();
            var client = new Core.Common.Models.Client
            {
                IdTokenSignedResponseAlg = Jwt.Constants.JwsAlgNames.RS256,
                IdTokenEncryptedResponseAlg = Jwt.Constants.JweAlgNames.RSA1_5,
                IdTokenEncryptedResponseEnc = Jwt.Constants.JweEncNames.A128CBC_HS256
            };
            _clientRepositoryStub.Setup(c => c.GetClientByIdAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(client));

            // ACT
            await _clientHelper.GenerateIdTokenAsync("client_id", new JwsPayload());

            // ASSERT
            _jwtGeneratorStub.Verify(j => j.SignAsync(It.IsAny<JwsPayload>(), JwsAlg.RS256));
            _jwtGeneratorStub.Verify(j => j.EncryptAsync(It.IsAny<string>(), JweAlg.RSA1_5, JweEnc.A128CBC_HS256));
        }

        private void InitializeFakeObjects()
        {
            _clientRepositoryStub = new Mock<IClientRepository>();
            _jwtGeneratorStub = new Mock<IJwtGenerator>();
            _jwtParserStub = new Mock<IJwtParser>();
            _clientHelper = new ClientHelper(
                _clientRepositoryStub.Object,
                _jwtGeneratorStub.Object,
                _jwtParserStub.Object);
        }
    }
}
