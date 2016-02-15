using System;
using Moq;

using NUnit.Framework;

using SimpleIdentityServer.Core.Authenticate;
using SimpleIdentityServer.Core.Configuration;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Extensions;
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Core.Jwt.Signature;
using SimpleIdentityServer.Core.JwtToken;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Validators;

namespace SimpleIdentityServer.Core.UnitTests.Authenticate
{
    [TestFixture]
    public sealed class ClientAssertionAuthenticationFixture
    {
        private Mock<IJwsParser> _jwsParserFake;
        
        private Mock<ISimpleIdentityServerConfigurator> _simpleIdentityServerConfiguratorFake;

        private Mock<IClientValidator> _clientValidatorFake;

        private Mock<IJwtParser> _jwtParserFake;

        private IClientAssertionAuthentication _clientAssertionAuthentication;

        #region AuthenticateClientWithPrivateKeyJwt

        [Test]
        public void When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            string errorMessage;

            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(
                () => _clientAssertionAuthentication.AuthenticateClientWithPrivateKeyJwt(null, out errorMessage));
        }

        [Test]
        public void When_A_Not_Jws_Token_Is_Passed_To_AuthenticateClientWithPrivateKeyJwt_Then_Null_Is_Returned()
        {            
            // ARRANGE
            InitializeFakeObjects();
            string errorMessage;
            var instruction = new AuthenticateInstruction
            {
                ClientAssertion = "invalid_header.invalid_payload"
            };
            _jwtParserFake.Setup(j => j.IsJwsToken(It.IsAny<string>()))
                .Returns(false);

            // ACT
            var result = _clientAssertionAuthentication.AuthenticateClientWithPrivateKeyJwt(instruction, out errorMessage);

            // ASSERT
            Assert.IsNull(result);
            Assert.IsTrue(errorMessage == ErrorDescriptions.TheClientAssertionIsNotAJwsToken);
        }

        [Test]
        public void When_A_Jws_Token_With_Not_Payload_Is_Passed_To_AuthenticateClientWithPrivateKeyJwt_Then_Null_Is_Returned()
        {            
            // ARRANGE
            InitializeFakeObjects();
            string errorMessage;
            var instruction = new AuthenticateInstruction
            {
                ClientAssertion = "invalid_header.invalid_payload"
            };
            _jwtParserFake.Setup(j => j.IsJwsToken(It.IsAny<string>()))
                .Returns(true);
            _jwsParserFake.Setup(j => j.GetPayload(It.IsAny<string>()))
                .Returns(() => null);

            // ACT
            var result = _clientAssertionAuthentication.AuthenticateClientWithPrivateKeyJwt(instruction, out errorMessage);

            // ASSERT
            Assert.IsNull(result);
            Assert.IsTrue(errorMessage == ErrorDescriptions.TheJwsPayloadCannotBeExtracted);
        }

        [Test]
        public void When_A_Jws_Token_With_Invalid_Signature_Is_Passed_To_AuthenticateClientWithPrivateKeyJwt_Then_Null_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            string errorMessage;
            var instruction = new AuthenticateInstruction
            {
                ClientAssertion = "invalid_header.invalid_payload"
            };
            var jwsPayload = new JwsPayload();
            _jwtParserFake.Setup(j => j.IsJwsToken(It.IsAny<string>()))
                .Returns(true);
            _jwsParserFake.Setup(j => j.GetPayload(It.IsAny<string>()))
                .Returns(jwsPayload);
            _jwtParserFake.Setup(j => j.UnSign(It.IsAny<string>(),
                It.IsAny<string>()))
                .Returns(() => null);

            // ACT
            var result = _clientAssertionAuthentication.AuthenticateClientWithPrivateKeyJwt(instruction, out errorMessage);

            // ASSERT
            Assert.IsNull(result);
            Assert.IsTrue(errorMessage == ErrorDescriptions.TheSignatureIsNotCorrect);
        }

        [Test]
        public void When_A_Jws_Token_With_Invalid_Issuer_Is_Passed_To_AuthenticateClientWithPrivateKeyJwt_Then_Null_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            string errorMessage;
            var instruction = new AuthenticateInstruction
            {
                ClientAssertion = "invalid_header.invalid_payload"
            };
            var jwsPayload = new JwsPayload
            {
                {
                    Jwt.Constants.StandardClaimNames.Issuer, "issuer"
                }
            };
            _jwtParserFake.Setup(j => j.IsJwsToken(It.IsAny<string>()))
                .Returns(true);
            _jwsParserFake.Setup(j => j.GetPayload(It.IsAny<string>()))
                .Returns(jwsPayload);
            _jwtParserFake.Setup(j => j.UnSign(It.IsAny<string>(),
                It.IsAny<string>()))
                .Returns(jwsPayload);
            _clientValidatorFake.Setup(c => c.ValidateClientExist(It.IsAny<string>()))
                .Returns(() => null);

            // ACT
            var result = _clientAssertionAuthentication.AuthenticateClientWithPrivateKeyJwt(instruction, out errorMessage);

            // ASSERT
            Assert.IsNull(result);
            Assert.IsTrue(errorMessage == ErrorDescriptions.TheClientIdPassedInJwtIsNotCorrect);
        }

        [Test]
        public void When_A_Jws_Token_With_Invalid_Audience_Is_Passed_To_AuthenticateClientWithPrivateKeyJwt_Then_Null_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            string errorMessage;
            var instruction = new AuthenticateInstruction
            {
                ClientAssertion = "invalid_header.invalid_payload"
            };
            var jwsPayload = new JwsPayload
            {
                {
                    Jwt.Constants.StandardClaimNames.Issuer, "issuer"
                },
                {
                    Jwt.Constants.StandardResourceOwnerClaimNames.Subject, "issuer"
                },
                {
                    Jwt.Constants.StandardClaimNames.Audiences, new []
                    {
                        "audience"   
                    }
                }
            };
            var client = new Client();
            _jwtParserFake.Setup(j => j.IsJwsToken(It.IsAny<string>()))
                .Returns(true);
            _jwsParserFake.Setup(j => j.GetPayload(It.IsAny<string>()))
                .Returns(jwsPayload);
            _jwtParserFake.Setup(j => j.UnSign(It.IsAny<string>(),
                It.IsAny<string>()))
                .Returns(jwsPayload);
            _clientValidatorFake.Setup(c => c.ValidateClientExist(It.IsAny<string>()))
                .Returns(client);
            _simpleIdentityServerConfiguratorFake.Setup(s => s.GetIssuerName())
                .Returns("invalid_issuer");

            // ACT
            var result = _clientAssertionAuthentication.AuthenticateClientWithPrivateKeyJwt(instruction, out errorMessage);

            // ASSERT
            Assert.IsNull(result);
            Assert.IsTrue(errorMessage == ErrorDescriptions.TheAudiencePassedInJwtIsNotCorrect);
        }

        [Test]
        public void When_An_Expired_Jws_Token_Is_Passed_To_AuthenticateClientWithPrivateKeyJwt_Then_Null_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            string errorMessage;
            var instruction = new AuthenticateInstruction
            {
                ClientAssertion = "invalid_header.invalid_payload"
            };
            var jwsPayload = new JwsPayload
            {
                {
                    Jwt.Constants.StandardClaimNames.Issuer, "issuer"
                },
                {
                    Jwt.Constants.StandardResourceOwnerClaimNames.Subject, "issuer"
                },
                {
                    Jwt.Constants.StandardClaimNames.Audiences, new []
                    {
                        "audience"   
                    }
                },
                {
                    Jwt.Constants.StandardClaimNames.ExpirationTime, DateTime.Now.AddDays(-2)
                }
            };
            var client = new Client();
            _jwtParserFake.Setup(j => j.IsJwsToken(It.IsAny<string>()))
                .Returns(true);
            _jwsParserFake.Setup(j => j.GetPayload(It.IsAny<string>()))
                .Returns(jwsPayload);
            _jwtParserFake.Setup(j => j.UnSign(It.IsAny<string>(),
                It.IsAny<string>()))
                .Returns(jwsPayload);
            _clientValidatorFake.Setup(c => c.ValidateClientExist(It.IsAny<string>()))
                .Returns(client);
            _simpleIdentityServerConfiguratorFake.Setup(s => s.GetIssuerName())
                .Returns("audience");

            // ACT
            var result = _clientAssertionAuthentication.AuthenticateClientWithPrivateKeyJwt(instruction, out errorMessage);

            // ASSERT
            Assert.IsNull(result);
            Assert.IsTrue(errorMessage == ErrorDescriptions.TheReceivedJwtHasExpired);
        }

        [Test]
        public void When_A_Valid_Jws_Token_Is_Passed_To_AuthenticateClientWithPrivateKeyJwt_Then_Client_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            string errorMessage;
            var instruction = new AuthenticateInstruction
            {
                ClientAssertion = "invalid_header.invalid_payload"
            };
            var jwsPayload = new JwsPayload
            {
                {
                    Jwt.Constants.StandardClaimNames.Issuer, "issuer"
                },
                {
                    Jwt.Constants.StandardResourceOwnerClaimNames.Subject, "issuer"
                },
                {
                    Jwt.Constants.StandardClaimNames.Audiences, new []
                    {
                        "audience"   
                    }
                },
                {
                    Jwt.Constants.StandardClaimNames.ExpirationTime, DateTime.UtcNow.AddDays(2).ConvertToUnixTimestamp()
                }
            };
            var client = new Client();
            _jwtParserFake.Setup(j => j.IsJwsToken(It.IsAny<string>()))
                .Returns(true);
            _jwsParserFake.Setup(j => j.GetPayload(It.IsAny<string>()))
                .Returns(jwsPayload);
            _jwtParserFake.Setup(j => j.UnSign(It.IsAny<string>(),
                It.IsAny<string>()))
                .Returns(jwsPayload);
            _clientValidatorFake.Setup(c => c.ValidateClientExist(It.IsAny<string>()))
                .Returns(client);
            _simpleIdentityServerConfiguratorFake.Setup(s => s.GetIssuerName())
                .Returns("audience");

            // ACT
            var result = _clientAssertionAuthentication.AuthenticateClientWithPrivateKeyJwt(instruction, out errorMessage);

            // ASSERT
            Assert.IsNotNull(result);
        }

        #endregion

        #region AuthenticateClientWithClientSecretJwt

        [Test]
        public void When_Passing_Null_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            string messageError;

            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(() => _clientAssertionAuthentication.AuthenticateClientWithClientSecretJwt(null, string.Empty, out messageError));
        }

        [Test]
        public void When_Passing_A_Not_Jwe_Token_To_AuthenticateClientWithClientSecretJwt_Then_Null_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            string errorMessage;
            var instruction = new AuthenticateInstruction
            {
                ClientAssertion = "valid_header.valid.valid.valid.valid"
            };
            _jwtParserFake.Setup(j => j.IsJweToken(It.IsAny<string>()))
                .Returns(false);
            
            // ACT
            var client = _clientAssertionAuthentication.AuthenticateClientWithClientSecretJwt(instruction,
                string.Empty,
                out errorMessage);

            // ASSERT
            Assert.IsNull(client);
            Assert.IsTrue(errorMessage == ErrorDescriptions.TheClientAssertionIsNotAJweToken);
        }

        [Test]
        public void When_Passing_A_Not_Valid_Jwe_Token_To_AuthenticateClientWithClientSecretJwt_Then_Null_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            string errorMessage;
            var instruction = new AuthenticateInstruction
            {
                ClientAssertion = "valid_header.valid.valid.valid.valid"
            };
            _jwtParserFake.Setup(j => j.IsJweToken(It.IsAny<string>()))
                .Returns(true);
            _jwtParserFake.Setup(j => j.DecryptWithPassword(It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
                .Returns(string.Empty);

            // ACT
            var client = _clientAssertionAuthentication.AuthenticateClientWithClientSecretJwt(instruction,
                string.Empty,
                out errorMessage);

            // ASSERT
            Assert.IsNull(client);
            Assert.IsTrue(errorMessage == ErrorDescriptions.TheJweTokenCannotBeDecrypted);
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
            _jwtParserFake.Setup(j => j.IsJweToken(It.IsAny<string>()))
                .Returns(true);
            _jwtParserFake.Setup(j => j.DecryptWithPassword(It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
                .Returns("jws");
            _jwtParserFake.Setup(j => j.IsJwsToken(It.IsAny<string>()))
                .Returns(false);

            // ACT
            var client = _clientAssertionAuthentication.AuthenticateClientWithClientSecretJwt(instruction,
                string.Empty,
                out errorMessage);

            // ASSERT
            Assert.IsNull(client);
            Assert.IsTrue(errorMessage == ErrorDescriptions.TheClientAssertionIsNotAJwsToken);
        }

        [Test]
        public void When_Decrypt_Client_Secret_Jwt_And_Cannot_Extract_Jws_PayLoad_Then_Null_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            string errorMessage;
            var instruction = new AuthenticateInstruction
            {
                ClientAssertion = "valid_header.valid.valid.valid.valid"
            };
            _jwtParserFake.Setup(j => j.IsJweToken(It.IsAny<string>()))
                .Returns(true);
            _jwtParserFake.Setup(j => j.DecryptWithPassword(It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
                .Returns("jws");
            _jwtParserFake.Setup(j => j.IsJwsToken(It.IsAny<string>()))
                .Returns(true);
            _jwtParserFake.Setup(j => j.UnSign(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(() => null);

            // ACT
            var client = _clientAssertionAuthentication.AuthenticateClientWithClientSecretJwt(instruction,
                string.Empty,
                out errorMessage);

            // ASSERT
            Assert.IsNull(client);
            Assert.IsTrue(errorMessage == ErrorDescriptions.TheJwsPayloadCannotBeExtracted);
        }
        
        [Test]
        public void When_Decrypt_Valid_Client_Secret_Jwt_Then_Client_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            string errorMessage;
            var instruction = new AuthenticateInstruction
            {
                ClientAssertion = "valid_header.valid.valid.valid.valid"
            };
            var jwsPayload = new JwsPayload
            {
                {
                    Jwt.Constants.StandardClaimNames.Issuer, "issuer"
                },
                {
                    Jwt.Constants.StandardResourceOwnerClaimNames.Subject, "issuer"
                },
                {
                    Jwt.Constants.StandardClaimNames.Audiences, new []
                    {
                        "audience"   
                    }
                },
                {
                    Jwt.Constants.StandardClaimNames.ExpirationTime, DateTime.Now.AddDays(2).ConvertToUnixTimestamp()
                }
            };
            var client = new Client();
            _jwtParserFake.Setup(j => j.IsJweToken(It.IsAny<string>()))
                .Returns(true);
            _jwtParserFake.Setup(j => j.DecryptWithPassword(It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
                .Returns("jws");
            _jwtParserFake.Setup(j => j.IsJwsToken(It.IsAny<string>()))
                .Returns(true);
            _jwtParserFake.Setup(j => j.UnSign(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(jwsPayload);
            _clientValidatorFake.Setup(c => c.ValidateClientExist(It.IsAny<string>()))
                .Returns(client);
            _simpleIdentityServerConfiguratorFake.Setup(s => s.GetIssuerName())
                .Returns("audience");

            // ACT
            var result = _clientAssertionAuthentication.AuthenticateClientWithClientSecretJwt(instruction,
                string.Empty,
                out errorMessage);

            // ASSERT
            Assert.IsNotNull(result);
        }

        #endregion

        private void InitializeFakeObjects()
        {
            _jwsParserFake = new Mock<IJwsParser>();
            _simpleIdentityServerConfiguratorFake = new Mock<ISimpleIdentityServerConfigurator>();
            _clientValidatorFake = new Mock<IClientValidator>();
            _jwtParserFake = new Mock<IJwtParser>();
            _clientAssertionAuthentication = new ClientAssertionAuthentication(
                _jwsParserFake.Object,
                _simpleIdentityServerConfiguratorFake.Object,
                _clientValidatorFake.Object,
                _jwtParserFake.Object);
        }
    }
}
