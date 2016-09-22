using System;
using Moq;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Core.JwtToken;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Validators;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.Helpers
{
    public sealed class ClientHelperFixture
    {
        private Mock<IClientValidator> _clientValidatorFake;

        private Mock<IJwtGenerator> _jwtGeneratorFake;

        private IClientHelper _clientHelper;

        [Fact]
        public void When_Passing_Null_Parameters_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERTS
            Assert.Throws<ArgumentNullException>(() => _clientHelper.GenerateIdToken(string.Empty, null));
            Assert.Throws<ArgumentNullException>(() => _clientHelper.GenerateIdToken("client_id", null));
        }

        [Fact]
        public void When_Signed_Response_Alg_Is_Not_Passed_Then_RS256_Is_Used()
        {
            // ARRANGE
            InitializeFakeObjects();
            var client = new Models.Client();
            _clientValidatorFake.Setup(c => c.ValidateClientExist(It.IsAny<string>()))
                .Returns(client);

            // ACT
            _clientHelper.GenerateIdToken("client_id", new JwsPayload());

            // ASSERT
            _jwtGeneratorFake.Verify(j => j.Sign(It.IsAny<JwsPayload>(), JwsAlg.RS256));
        }

        [Fact]
        public void When_Signed_Response_And_EncryptResponseAlg_Are_Passed_Then_EncryptResponseEnc_A128CBC_HS256_Is_Used()
        {
            // ARRANGE
            InitializeFakeObjects();
            var client = new Models.Client
            {
                IdTokenSignedResponseAlg = Jwt.Constants.JwsAlgNames.RS256,
                IdTokenEncryptedResponseAlg = Jwt.Constants.JweAlgNames.RSA1_5
            };
            _clientValidatorFake.Setup(c => c.ValidateClientExist(It.IsAny<string>()))
                .Returns(client);

            // ACT
            _clientHelper.GenerateIdToken("client_id", new JwsPayload());

            // ASSERT
            _jwtGeneratorFake.Verify(j => j.Sign(It.IsAny<JwsPayload>(), JwsAlg.RS256));
            _jwtGeneratorFake.Verify(j => j.Encrypt(It.IsAny<string>(), JweAlg.RSA1_5, JweEnc.A128CBC_HS256));
        }
        
        [Fact]
        public void When_Sign_And_Encrypt_JwsPayload_Then_Functions_Are_Called()
        {
            // ARRANGE
            InitializeFakeObjects();
            var client = new Models.Client
            {
                IdTokenSignedResponseAlg = Jwt.Constants.JwsAlgNames.RS256,
                IdTokenEncryptedResponseAlg = Jwt.Constants.JweAlgNames.RSA1_5,
                IdTokenEncryptedResponseEnc = Jwt.Constants.JweEncNames.A128CBC_HS256
            };
            _clientValidatorFake.Setup(c => c.ValidateClientExist(It.IsAny<string>()))
                .Returns(client);

            // ACT
            _clientHelper.GenerateIdToken("client_id", new JwsPayload());

            // ASSERT
            _jwtGeneratorFake.Verify(j => j.Sign(It.IsAny<JwsPayload>(), JwsAlg.RS256));
            _jwtGeneratorFake.Verify(j => j.Encrypt(It.IsAny<string>(), JweAlg.RSA1_5, JweEnc.A128CBC_HS256));
        }

        private void InitializeFakeObjects()
        {
            _clientValidatorFake = new Mock<IClientValidator>();
            _jwtGeneratorFake = new Mock<IJwtGenerator>();
            _clientHelper = new ClientHelper(
                _clientValidatorFake.Object,
                _jwtGeneratorFake.Object);
        }
    }
}
