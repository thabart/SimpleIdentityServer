using Moq;
using SimpleIdentityServer.Core.Authenticate;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Extensions;
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Core.Jwt.Signature;
using SimpleIdentityServer.Core.JwtToken;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Core.Services;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.Authenticate
{
    public sealed class ClientAssertionAuthenticationFixture
    {
        private Mock<IJwsParser> _jwsParserFake;        
        private Mock<IConfigurationService> _simpleIdentityServerConfiguratorFake;
        private Mock<IClientRepository> _clientRepositoryStub;
        private Mock<IJwtParser> _jwtParserFake;
        private IClientAssertionAuthentication _clientAssertionAuthentication;
        
        #region AuthenticateClientWithPrivateKeyJwt

        [Fact]
        public async Task When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            await Assert.ThrowsAsync<ArgumentNullException>(() => _clientAssertionAuthentication.AuthenticateClientWithPrivateKeyJwtAsync(null)).ConfigureAwait(false);
        }

        [Fact]
        public async Task When_A_Not_Jws_Token_Is_Passed_To_AuthenticateClientWithPrivateKeyJwt_Then_Null_Is_Returned()
        {            
            // ARRANGE
            InitializeFakeObjects();
            var instruction = new AuthenticateInstruction
            {
                ClientAssertion = "invalid_header.invalid_payload"
            };
            _jwtParserFake.Setup(j => j.IsJwsToken(It.IsAny<string>()))
                .Returns(false);

            // ACT
            var result = await _clientAssertionAuthentication.AuthenticateClientWithPrivateKeyJwtAsync(instruction).ConfigureAwait(false);

            // ASSERT
            Assert.Null(result.Client);
            Assert.True(result.ErrorMessage == ErrorDescriptions.TheClientAssertionIsNotAJwsToken);
        }

        [Fact]
        public async Task When_A_Jws_Token_With_Not_Payload_Is_Passed_To_AuthenticateClientWithPrivateKeyJwt_Then_Null_Is_Returned()
        {            
            // ARRANGE
            InitializeFakeObjects();
            var instruction = new AuthenticateInstruction
            {
                ClientAssertion = "invalid_header.invalid_payload"
            };
            _jwtParserFake.Setup(j => j.IsJwsToken(It.IsAny<string>()))
                .Returns(true);
            _jwsParserFake.Setup(j => j.GetPayload(It.IsAny<string>()))
                .Returns(() => null);

            // ACT
            var result = await _clientAssertionAuthentication.AuthenticateClientWithPrivateKeyJwtAsync(instruction).ConfigureAwait(false);

            // ASSERT
            Assert.Null(result.Client);
            Assert.True(result.ErrorMessage == ErrorDescriptions.TheJwsPayloadCannotBeExtracted);
        }

        [Fact]
        public async Task When_A_Jws_Token_With_Invalid_Signature_Is_Passed_To_AuthenticateClientWithPrivateKeyJwt_Then_Null_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            var instruction = new AuthenticateInstruction
            {
                ClientAssertion = "invalid_header.invalid_payload"
            };
            var jwsPayload = new JwsPayload();
            _jwtParserFake.Setup(j => j.IsJwsToken(It.IsAny<string>()))
                .Returns(true);
            _jwsParserFake.Setup(j => j.GetPayload(It.IsAny<string>()))
                .Returns(jwsPayload);
            _jwtParserFake.Setup(j => j.UnSignAsync(It.IsAny<string>(),
                It.IsAny<string>()))
                .Returns(() => Task.FromResult((JwsPayload)null));

            // ACT
            var result = await _clientAssertionAuthentication.AuthenticateClientWithPrivateKeyJwtAsync(instruction).ConfigureAwait(false);

            // ASSERT
            Assert.Null(result.Client);
            Assert.True(result.ErrorMessage == ErrorDescriptions.TheSignatureIsNotCorrect);
        }

        [Fact]
        public async Task When_A_Jws_Token_With_Invalid_Issuer_Is_Passed_To_AuthenticateClientWithPrivateKeyJwt_Then_Null_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
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
            _jwtParserFake.Setup(j => j.UnSignAsync(It.IsAny<string>(),
                It.IsAny<string>()))
                .Returns(Task.FromResult(jwsPayload));
            _clientRepositoryStub.Setup(c => c.GetClientByIdAsync(It.IsAny<string>()))
                .Returns(() => Task.FromResult((Models.Client)null));

            // ACT
            var result = await _clientAssertionAuthentication.AuthenticateClientWithPrivateKeyJwtAsync(instruction).ConfigureAwait(false);

            // ASSERT
            Assert.Null(result.Client);
            Assert.True(result.ErrorMessage == ErrorDescriptions.TheClientIdPassedInJwtIsNotCorrect);
        }

        [Fact]
        public async Task When_A_Jws_Token_With_Invalid_Audience_Is_Passed_To_AuthenticateClientWithPrivateKeyJwt_Then_Null_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
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
            var client = new Models.Client();
            _jwtParserFake.Setup(j => j.IsJwsToken(It.IsAny<string>()))
                .Returns(true);
            _jwsParserFake.Setup(j => j.GetPayload(It.IsAny<string>()))
                .Returns(jwsPayload);
            _jwtParserFake.Setup(j => j.UnSignAsync(It.IsAny<string>(),
                It.IsAny<string>()))
                .Returns(Task.FromResult(jwsPayload));
            _clientRepositoryStub.Setup(c => c.GetClientByIdAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(client));
            _simpleIdentityServerConfiguratorFake.Setup(s => s.GetIssuerNameAsync())
                .Returns(Task.FromResult("invalid_issuer"));

            // ACT
            var result = await _clientAssertionAuthentication.AuthenticateClientWithPrivateKeyJwtAsync(instruction).ConfigureAwait(false);

            // ASSERT
            Assert.Null(result.Client);
            Assert.True(result.ErrorMessage == ErrorDescriptions.TheAudiencePassedInJwtIsNotCorrect);
        }

        [Fact]
        public async Task When_An_Expired_Jws_Token_Is_Passed_To_AuthenticateClientWithPrivateKeyJwt_Then_Null_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
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
            var client = new Models.Client();
            _jwtParserFake.Setup(j => j.IsJwsToken(It.IsAny<string>()))
                .Returns(true);
            _jwsParserFake.Setup(j => j.GetPayload(It.IsAny<string>()))
                .Returns(jwsPayload);
            _jwtParserFake.Setup(j => j.UnSignAsync(It.IsAny<string>(),
                It.IsAny<string>()))
                .Returns(Task.FromResult(jwsPayload));
            _clientRepositoryStub.Setup(c => c.GetClientByIdAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(client));
            _simpleIdentityServerConfiguratorFake.Setup(s => s.GetIssuerNameAsync())
                .Returns(Task.FromResult("audience"));

            // ACT
            var result = await _clientAssertionAuthentication.AuthenticateClientWithPrivateKeyJwtAsync(instruction).ConfigureAwait(false);

            // ASSERT
            Assert.Null(result.Client);
            Assert.True(result.ErrorMessage == ErrorDescriptions.TheReceivedJwtHasExpired);
        }

        [Fact]
        public async Task When_A_Valid_Jws_Token_Is_Passed_To_AuthenticateClientWithPrivateKeyJwt_Then_Client_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
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
            var client = new Models.Client();
            _jwtParserFake.Setup(j => j.IsJwsToken(It.IsAny<string>()))
                .Returns(true);
            _jwsParserFake.Setup(j => j.GetPayload(It.IsAny<string>()))
                .Returns(jwsPayload);
            _jwtParserFake.Setup(j => j.UnSignAsync(It.IsAny<string>(),
                It.IsAny<string>()))
                .Returns(Task.FromResult(jwsPayload));
            _clientRepositoryStub.Setup(c => c.GetClientByIdAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(client));
            _simpleIdentityServerConfiguratorFake.Setup(s => s.GetIssuerNameAsync())
                .Returns(Task.FromResult("audience"));

            // ACT
            var result = await _clientAssertionAuthentication.AuthenticateClientWithPrivateKeyJwtAsync(instruction).ConfigureAwait(false);

            // ASSERT
            Assert.NotNull(result.Client);
        }

        #endregion

        #region AuthenticateClientWithClientSecretJwt

        [Fact]
        public async Task When_Passing_Null_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            await Assert.ThrowsAsync<ArgumentNullException>(() => _clientAssertionAuthentication.AuthenticateClientWithClientSecretJwtAsync(null, string.Empty)).ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Passing_A_Not_Jwe_Token_To_AuthenticateClientWithClientSecretJwt_Then_Null_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            var instruction = new AuthenticateInstruction
            {
                ClientAssertion = "valid_header.valid.valid.valid.valid"
            };
            _jwtParserFake.Setup(j => j.IsJweToken(It.IsAny<string>()))
                .Returns(false);
            
            // ACT
            var result = await _clientAssertionAuthentication.AuthenticateClientWithClientSecretJwtAsync(instruction, string.Empty).ConfigureAwait(false);

            // ASSERT
            Assert.Null(result.Client);
            Assert.True(result.ErrorMessage == ErrorDescriptions.TheClientAssertionIsNotAJweToken);
        }

        [Fact]
        public async Task When_Passing_A_Not_Valid_Jwe_Token_To_AuthenticateClientWithClientSecretJwt_Then_Null_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            var instruction = new AuthenticateInstruction
            {
                ClientAssertion = "valid_header.valid.valid.valid.valid"
            };
            _jwtParserFake.Setup(j => j.IsJweToken(It.IsAny<string>()))
                .Returns(true);
            _jwtParserFake.Setup(j => j.DecryptWithPasswordAsync(It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
                .Returns(Task.FromResult(string.Empty));

            // ACT
            var result = await _clientAssertionAuthentication.AuthenticateClientWithClientSecretJwtAsync(instruction, string.Empty).ConfigureAwait(false);

            // ASSERT
            Assert.Null(result.Client);
            Assert.True(result.ErrorMessage == ErrorDescriptions.TheJweTokenCannotBeDecrypted);
        }

        [Fact]
        public async Task When_Decrypt_Client_Secret_Jwt_And_Its_Not_A_Jws_Token_Then_Null_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            var instruction = new AuthenticateInstruction
            {
                ClientAssertion = "valid_header.valid.valid.valid.valid"
            };
            _jwtParserFake.Setup(j => j.IsJweToken(It.IsAny<string>()))
                .Returns(true);
            _jwtParserFake.Setup(j => j.DecryptWithPasswordAsync(It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
                .Returns(Task.FromResult("jws"));
            _jwtParserFake.Setup(j => j.IsJwsToken(It.IsAny<string>()))
                .Returns(false);

            // ACT
            var result = await _clientAssertionAuthentication.AuthenticateClientWithClientSecretJwtAsync(instruction, string.Empty).ConfigureAwait(false);

            // ASSERT
            Assert.Null(result.Client);
            Assert.True(result.ErrorMessage == ErrorDescriptions.TheClientAssertionIsNotAJwsToken);
        }

        [Fact]
        public async Task When_Decrypt_Client_Secret_Jwt_And_Cannot_Extract_Jws_PayLoad_Then_Null_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            var instruction = new AuthenticateInstruction
            {
                ClientAssertion = "valid_header.valid.valid.valid.valid"
            };
            _jwtParserFake.Setup(j => j.IsJweToken(It.IsAny<string>()))
                .Returns(true);
            _jwtParserFake.Setup(j => j.DecryptWithPasswordAsync(It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
                .Returns(Task.FromResult("jws"));
            _jwtParserFake.Setup(j => j.IsJwsToken(It.IsAny<string>()))
                .Returns(true);
            _jwtParserFake.Setup(j => j.UnSignAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(() => Task.FromResult((JwsPayload)null));

            // ACT
            var result = await _clientAssertionAuthentication.AuthenticateClientWithClientSecretJwtAsync(instruction, string.Empty).ConfigureAwait(false);

            // ASSERT
            Assert.Null(result.Client);
            Assert.True(result.ErrorMessage == ErrorDescriptions.TheJwsPayloadCannotBeExtracted);
        }
        
        [Fact]
        public async Task When_Decrypt_Valid_Client_Secret_Jwt_Then_Client_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
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
            var client = new Models.Client();
            _jwtParserFake.Setup(j => j.IsJweToken(It.IsAny<string>()))
                .Returns(true);
            _jwtParserFake.Setup(j => j.DecryptWithPasswordAsync(It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
                .Returns(Task.FromResult("jws"));
            _jwtParserFake.Setup(j => j.IsJwsToken(It.IsAny<string>()))
                .Returns(true);
            _jwtParserFake.Setup(j => j.UnSignAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult(jwsPayload));
            _clientRepositoryStub.Setup(c => c.GetClientByIdAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(client));
            _simpleIdentityServerConfiguratorFake.Setup(s => s.GetIssuerNameAsync())
                .Returns(Task.FromResult("audience"));

            // ACT
            var result = await _clientAssertionAuthentication.AuthenticateClientWithClientSecretJwtAsync(instruction, string.Empty).ConfigureAwait(false);

            // ASSERT
            Assert.NotNull(result);
        }

        #endregion

        private void InitializeFakeObjects()
        {
            _jwsParserFake = new Mock<IJwsParser>();
            _simpleIdentityServerConfiguratorFake = new Mock<IConfigurationService>();
            _clientRepositoryStub = new Mock<IClientRepository>();
            _jwtParserFake = new Mock<IJwtParser>();
            _clientAssertionAuthentication = new ClientAssertionAuthentication(
                _jwsParserFake.Object,
                _simpleIdentityServerConfiguratorFake.Object,
                _clientRepositoryStub.Object,
                _jwtParserFake.Object);
        }
    }
}
