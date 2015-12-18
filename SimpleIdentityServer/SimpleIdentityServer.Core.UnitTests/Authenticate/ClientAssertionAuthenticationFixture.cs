using System;
using Moq;

using NUnit.Framework;

using SimpleIdentityServer.Core.Authenticate;
using SimpleIdentityServer.Core.Configuration;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Extensions;
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Core.Jwt.Encrypt;
using SimpleIdentityServer.Core.Jwt.Signature;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Core.Validators;

namespace SimpleIdentityServer.Core.UnitTests.Authenticate
{
    [TestFixture]
    public sealed class ClientAssertionAuthenticationFixture
    {
        private Mock<IJweParser> _jweParserFake;

        private Mock<IJwsParser> _jwsParserFake;

        private Mock<IJsonWebKeyRepository> _jsonWebKeyRepositoryFake;

        private Mock<ISimpleIdentityServerConfigurator> _simpleIdentityServerConfiguratorFake;

        private Mock<IClientValidator> _clientValidatorFake;

        private IClientAssertionAuthentication _clientAssertionAuthentication;

        #region AuthenticateClientWithPrivateKeyJwt

        [Test]
        public void When_Passing_No_Instruction_To_Authenticate_Client_With_His_Private_Key_Then_Null_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            string errorMessage;

            // ACT
            var client = _clientAssertionAuthentication.AuthenticateClientWithPrivateKeyJwt(null, out errorMessage);
            
            // ASSERTS
            Assert.IsTrue(errorMessage == ErrorDescriptions.TheClientAssertionIsNotAJwsToken);
            Assert.IsNull(client);
        }

        [Test]
        public void When_Passing_Invalid_Private_Key_Jwt_To_Authenticate_The_Client_Then_Null_Is_Returned()
        {            
            // ARRANGE
            InitializeFakeObjects();
            string errorMessage;
            var instruction = new AuthenticateInstruction
            {
                ClientAssertion = "invalid_header.invalid_payload"
            };

            // ACT
            var client = _clientAssertionAuthentication.AuthenticateClientWithPrivateKeyJwt(instruction, out errorMessage);

            // ASSERTS
            Assert.IsTrue(errorMessage == ErrorDescriptions.TheClientAssertionIsNotAJwsToken);
            Assert.IsNull(client);
        }

        [Test]
        public void When_Passing_Private_Key_Jwt_With_Invalid_Header_Then_Null_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            string errorMessage;
            var instruction = new AuthenticateInstruction
            {
                ClientAssertion = "invalid_header.invalid_payload.invalid_signature"
            };
            _jwsParserFake.Setup(j => j.GetHeader(It.IsAny<string>()))
                .Returns(() => null);

            // ACT
            var client = _clientAssertionAuthentication.AuthenticateClientWithPrivateKeyJwt(instruction, out errorMessage);

            // ASSERTS
            Assert.IsTrue(errorMessage == ErrorDescriptions.TheHeaderCannotBeExtractedFromJwsToken);
            Assert.IsNull(client);
        }

        [Test]
        public void When_Jwt_Private_Key_Signature_Is_Not_Correct_Then_Null_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            string errorMessage;
            var instruction = new AuthenticateInstruction
            {
                ClientAssertion = "valid_header.valid_payload.valid_signature"
            };
            var jwsProtectedHeader = new JwsProtectedHeader();
            _jwsParserFake.Setup(j => j.GetHeader(It.IsAny<string>()))
                .Returns(jwsProtectedHeader);
            _jwsParserFake.Setup(j => j.ValidateSignature(It.IsAny<string>(), It.IsAny<JsonWebKey>()))
                .Returns(() => null);

            // ACT
            var client = _clientAssertionAuthentication.AuthenticateClientWithPrivateKeyJwt(instruction, out errorMessage);

            // ASSERTS
            Assert.IsTrue(errorMessage == ErrorDescriptions.TheSignatureIsNotCorrect);
            Assert.IsNull(client);
        }

        [Test]
        public void When_The_Client_Id_Passed_In_Jwt_Private_Key_Is_Not_Correct_Then_Null_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            string errorMessage;
            var instruction = new AuthenticateInstruction
            {
                ClientAssertion = "valid_header.valid_payload.valid_signature"
            };
            var jwsProtectedHeader = new JwsProtectedHeader();
            var jwsPayload = new JwsPayload();
            jwsPayload.Add(Jwt.Constants.StandardResourceOwnerClaimNames.Subject, "fakeClientId");
            _jwsParserFake.Setup(j => j.GetHeader(It.IsAny<string>()))
                .Returns(jwsProtectedHeader);
            _jwsParserFake.Setup(j => j.ValidateSignature(It.IsAny<string>(), It.IsAny<JsonWebKey>()))
                .Returns(jwsPayload);
            _clientValidatorFake.Setup(c => c.ValidateClientExist(It.IsAny<string>()))
                .Returns(() => null);

            // ACT
            var client = _clientAssertionAuthentication.AuthenticateClientWithPrivateKeyJwt(instruction, out errorMessage);

            // ASSERTS
            Assert.IsTrue(errorMessage == ErrorDescriptions.TheClientIdPassedInJwtIsNotCorrect);
            Assert.IsNull(client);
        }

        [Test]
        public void When_The_Issuer_And_Subject_Are_Not_The_Them_Then_Null_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            string errorMessage;
            const string validClientId = "clientId";
            var instruction = new AuthenticateInstruction
            {
                ClientAssertion = "valid_header.valid_payload.valid_signature"
            };
            var jwsProtectedHeader = new JwsProtectedHeader();
            var jwsPayload = new JwsPayload();
            var client = new Client
            {
                ClientId = validClientId
            };
            jwsPayload.Add(Jwt.Constants.StandardResourceOwnerClaimNames.Subject, "fakeClientId");
            jwsPayload.Add(Jwt.Constants.StandardClaimNames.Issuer, validClientId);
            _jwsParserFake.Setup(j => j.GetHeader(It.IsAny<string>()))
                .Returns(jwsProtectedHeader);
            _jwsParserFake.Setup(j => j.ValidateSignature(It.IsAny<string>(), It.IsAny<JsonWebKey>()))
                .Returns(jwsPayload);
            _clientValidatorFake.Setup(c => c.ValidateClientExist(It.IsAny<string>()))
                .Returns(client);

            // ACT
            client = _clientAssertionAuthentication.AuthenticateClientWithPrivateKeyJwt(instruction, out errorMessage);

            // ASSERTS
            Assert.IsTrue(errorMessage == ErrorDescriptions.TheClientIdPassedInJwtIsNotCorrect);
            Assert.IsNull(client);
        }

        [Test]
        public void When_The_Identity_Server_Url_Doesnt_Belong_To_The_Audience_Then_Null_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            string errorMessage;
            const string validClientId = "clientId";
            var audiences = new [] {"audience"};
            const string simpleIdentityServerUrl = "http://localhost";
            var instruction = new AuthenticateInstruction
            {
                ClientAssertion = "valid_header.valid_payload.valid_signature"
            };
            var jwsProtectedHeader = new JwsProtectedHeader();
            var jwsPayload = new JwsPayload();
            var client = new Client
            {
                ClientId = validClientId
            };
            jwsPayload.Add(Jwt.Constants.StandardResourceOwnerClaimNames.Subject, validClientId);
            jwsPayload.Add(Jwt.Constants.StandardClaimNames.Issuer, validClientId);
            jwsPayload.Add(Jwt.Constants.StandardClaimNames.Audiences, audiences);
            _jwsParserFake.Setup(j => j.GetHeader(It.IsAny<string>()))
                .Returns(jwsProtectedHeader);
            _jwsParserFake.Setup(j => j.ValidateSignature(It.IsAny<string>(), It.IsAny<JsonWebKey>()))
                .Returns(jwsPayload);
            _clientValidatorFake.Setup(c => c.ValidateClientExist(It.IsAny<string>()))
                .Returns(client);
            _simpleIdentityServerConfiguratorFake.Setup(s => s.GetIssuerName())
                .Returns(simpleIdentityServerUrl);

            // ACT
            client = _clientAssertionAuthentication.AuthenticateClientWithPrivateKeyJwt(instruction, out errorMessage);

            // ASSERTS
            Assert.IsTrue(errorMessage == ErrorDescriptions.TheAudiencePassedInJwtIsNotCorrect);
            Assert.IsNull(client);
        }

        [Test]
        public void When_The_Private_Key_Jwt_Is_Expired_Then_Null_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            string errorMessage;
            const string validClientId = "clientId";
            const string simpleIdentityServerUrl = "http://localhost";
            var audiences = new[] { simpleIdentityServerUrl };
            var instruction = new AuthenticateInstruction
            {
                ClientAssertion = "valid_header.valid_payload.valid_signature"
            };
            var jwsProtectedHeader = new JwsProtectedHeader();
            var jwsPayload = new JwsPayload();
            var client = new Client
            {
                ClientId = validClientId
            };
            var expirationDateTime = DateTime.UtcNow.AddSeconds(-5).ConvertToUnixTimestamp();
            jwsPayload.Add(Jwt.Constants.StandardResourceOwnerClaimNames.Subject, validClientId);
            jwsPayload.Add(Jwt.Constants.StandardClaimNames.Issuer, validClientId);
            jwsPayload.Add(Jwt.Constants.StandardClaimNames.Audiences, audiences);
            jwsPayload.Add(Jwt.Constants.StandardClaimNames.ExpirationTime, expirationDateTime);
            _jwsParserFake.Setup(j => j.GetHeader(It.IsAny<string>()))
                .Returns(jwsProtectedHeader);
            _jwsParserFake.Setup(j => j.ValidateSignature(It.IsAny<string>(), It.IsAny<JsonWebKey>()))
                .Returns(jwsPayload);
            _clientValidatorFake.Setup(c => c.ValidateClientExist(It.IsAny<string>()))
                .Returns(client);
            _simpleIdentityServerConfiguratorFake.Setup(s => s.GetIssuerName())
                .Returns(simpleIdentityServerUrl);

            // ACT
            client = _clientAssertionAuthentication.AuthenticateClientWithPrivateKeyJwt(instruction, out errorMessage);

            // ASSERTS
            Assert.IsTrue(errorMessage == ErrorDescriptions.TheReceivedJwtHasExpired);
            Assert.IsNull(client);
        }

        #endregion

        #region AuthenticateClientWithClientSecretJwt

        [Test]
        public void When_Passing_No_Instruction_To_Authenticate_Client_With_Secret_Jwt_Then_Null_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            string errorMessage;
            
            // ACT
            var client = _clientAssertionAuthentication.AuthenticateClientWithClientSecretJwt(null,
                string.Empty,
                out errorMessage);

            // ASSERT
            Assert.IsNull(client);
            Assert.IsTrue(errorMessage == ErrorDescriptions.TheClientAssertionIsNotAJweToken);
        }

        [Test]
        public void When_The_Client_Secret_Jwt_Header_Is_Invalid_Then_Null_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            string errorMessage;
            var instruction = new AuthenticateInstruction
            {
                ClientAssertion = "invalid_header.invalid.invalid.invalid.invalid"
            };
            _jweParserFake.Setup(j => j.GetHeader(It.IsAny<string>()))
                .Returns(() => null);

            // ACT
            var client = _clientAssertionAuthentication.AuthenticateClientWithClientSecretJwt(instruction,
                string.Empty,
                out errorMessage);

            // ASSERT
            Assert.IsNull(client);
            Assert.IsTrue(errorMessage == ErrorDescriptions.TheHeaderCannotBeExtractedFromJweToken);
        }

        [Test]
        public void When_The_Client_Secret_Jwt_Cannot_Be_Decrypted_With_ClientSecret_Then_Null_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            string errorMessage;
            var instruction = new AuthenticateInstruction
            {
                ClientAssertion = "invalid_header.invalid.invalid.invalid.invalid"
            };
            var jweProtectedHeader = new JweProtectedHeader();
            _jweParserFake.Setup(j => j.GetHeader(It.IsAny<string>()))
                .Returns(jweProtectedHeader);
            _jweParserFake.Setup(s => s.ParseByUsingSymmetricPassword(It.IsAny<string>(),
                It.IsAny<JsonWebKey>(),
                It.IsAny<string>())).
                Returns(() => null);

            // ACT
            var client = _clientAssertionAuthentication.AuthenticateClientWithClientSecretJwt(instruction,
                string.Empty,
                out errorMessage);

            // ASSERT
            Assert.IsNull(client);
            Assert.IsTrue(errorMessage == ErrorDescriptions.TheClientAssertionCannotBeDecrypted);
        }

        [Test]
        public void When_Decrypt_Client_Secret_Jwt_And_Its_Not_A_Jws_Token_Then_Null_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            string errorMessage;
            var instruction = new AuthenticateInstruction
            {
                ClientAssertion = "valid_header.valid.valid.valid.valid"
            };
            var jweProtectedHeader = new JweProtectedHeader();
            _jweParserFake.Setup(j => j.GetHeader(It.IsAny<string>()))
                .Returns(jweProtectedHeader);
            _jweParserFake.Setup(s => s.ParseByUsingSymmetricPassword(It.IsAny<string>(),
                It.IsAny<JsonWebKey>(),
                It.IsAny<string>())).
                Returns("invalid_header.invalid_payload");

            // ACT
            var client = _clientAssertionAuthentication.AuthenticateClientWithClientSecretJwt(instruction,
                string.Empty,
                out errorMessage);

            // ASSERT
            Assert.IsNull(client);
            Assert.IsTrue(errorMessage == ErrorDescriptions.TheClientAssertionIsNotAJwsToken);
        }

        [Test]
        public void When_Decrypt_Client_Secret_Jwt_And_The_Jws_Header_Is_Not_Correct_Then_Null_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            string errorMessage;
            var instruction = new AuthenticateInstruction
            {
                ClientAssertion = "valid_header.valid.valid.valid.valid"
            };
            var jweProtectedHeader = new JweProtectedHeader();
            _jweParserFake.Setup(j => j.GetHeader(It.IsAny<string>()))
                .Returns(jweProtectedHeader);
            _jweParserFake.Setup(s => s.ParseByUsingSymmetricPassword(It.IsAny<string>(),
                It.IsAny<JsonWebKey>(),
                It.IsAny<string>())).
                Returns("invalid_header.invalid_payload.invalid_signature");
            _jwsParserFake.Setup(j => j.GetHeader(It.IsAny<string>()))
                .Returns(() => null);

            // ACT
            var client = _clientAssertionAuthentication.AuthenticateClientWithClientSecretJwt(instruction,
                string.Empty,
                out errorMessage);

            // ASSERT
            Assert.IsNull(client);
            Assert.IsTrue(errorMessage == ErrorDescriptions.TheHeaderCannotBeExtractedFromJwsToken);
        }


        [Test]
        public void When_Decrypt_Client_Secret_Jwt_And_Its_Signature_Is_Not_Correct_Then_Null_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            string errorMessage;
            var instruction = new AuthenticateInstruction
            {
                ClientAssertion = "valid_header.valid.valid.valid.valid"
            };
            var jweProtectedHeader = new JweProtectedHeader();
            var jwsProtectedHeader = new JwsProtectedHeader();
            _jweParserFake.Setup(j => j.GetHeader(It.IsAny<string>()))
                .Returns(jweProtectedHeader);
            _jweParserFake.Setup(s => s.ParseByUsingSymmetricPassword(It.IsAny<string>(),
                It.IsAny<JsonWebKey>(),
                It.IsAny<string>())).
                Returns("invalid_header.invalid_payload.invalid_signature");
            _jwsParserFake.Setup(j => j.GetHeader(It.IsAny<string>()))
                .Returns(jwsProtectedHeader);
            _jwsParserFake.Setup(j => j.ValidateSignature(It.IsAny<string>(),
                It.IsAny<JsonWebKey>())).Returns(() => null);

            // ACT
            var client = _clientAssertionAuthentication.AuthenticateClientWithClientSecretJwt(instruction,
                string.Empty,
                out errorMessage);

            // ASSERT
            Assert.IsNull(client);
            Assert.IsTrue(errorMessage == ErrorDescriptions.TheSignatureIsNotCorrect);
        }

        #endregion

        private void InitializeFakeObjects()
        {
            _jweParserFake = new Mock<IJweParser>();
            _jwsParserFake = new Mock<IJwsParser>();
            _jsonWebKeyRepositoryFake = new Mock<IJsonWebKeyRepository>();
            _simpleIdentityServerConfiguratorFake = new Mock<ISimpleIdentityServerConfigurator>();
            _clientValidatorFake = new Mock<IClientValidator>();
            _clientAssertionAuthentication = new ClientAssertionAuthentication(
                _jweParserFake.Object,
                _jwsParserFake.Object,
                _jsonWebKeyRepositoryFake.Object,
                _simpleIdentityServerConfiguratorFake.Object,
                _clientValidatorFake.Object);
        }
    }
}
