using Moq;
using NUnit.Framework;
using SimpleIdentityServer.Core.Api.UserInfo.Actions;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.JwtToken;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Core.Validators;
using System;

namespace SimpleIdentityServer.Core.UnitTests.Api.UserInfo
{
    [TestFixture]
    public sealed class GetJwsPayloadFixture
    {
        private Mock<IGrantedTokenValidator> _grantedTokenValidatorFake;

        private Mock<IGrantedTokenRepository> _grantedTokenRepositoryFake;

        private Mock<IJwtParser> _jwtParserFake;

        private Mock<IClientRepository> _clientRepositoryFake;

        private IGetJwsPayload _getJwsPayload;
        
        [Test]
        public void When_Pass_Empty_Access_Token_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(() => _getJwsPayload.Execute(null));
        }

        [Test]
        public void When_Access_Token_Is_Not_Valid_Then_Authorization_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            string errorMessageCode,
                errorMessageDescription;
            _grantedTokenValidatorFake.Setup(g => g.CheckAccessToken(It.IsAny<string>(), out errorMessageCode, out errorMessageDescription))
                .Returns(false);

            // ACT & ASSERT
            Assert.Throws<AuthorizationException>(() => _getJwsPayload.Execute("access_token"));
        }

        [Test]
        public void When_None_Is_Specified_Then_JwsPayload_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            string errorMessageCode,
                errorMessageDescription;
            var grantedToken = new GrantedToken
            {
                UserInfoPayLoad = new Jwt.JwsPayload()
            };
            var client = new Client
            {
                UserInfoSignedResponseAlg = Jwt.Constants.JwsAlgNames.NONE
            };
            _grantedTokenValidatorFake.Setup(g => g.CheckAccessToken(It.IsAny<string>(), out errorMessageCode, out errorMessageDescription))
                .Returns(true);
            _grantedTokenRepositoryFake.Setup(g => g.GetToken(It.IsAny<string>()))
                .Returns(grantedToken);
            _clientRepositoryFake.Setup(c => c.GetClientById(It.IsAny<string>()))
                .Returns(client);

            // ACT
            var result = _getJwsPayload.Execute("access_token");

            // ASSERT
            Assert.IsNotNull(result);
        }

        [Test]
        public void When_There_Is_No_Algorithm_Specified_Then_JwsPayload_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            string errorMessageCode,
                errorMessageDescription;
            var grantedToken = new GrantedToken
            {
                UserInfoPayLoad = new Jwt.JwsPayload()
            };
            var client = new Client
            {
                UserInfoSignedResponseAlg = string.Empty
            };
            _grantedTokenValidatorFake.Setup(g => g.CheckAccessToken(It.IsAny<string>(), out errorMessageCode, out errorMessageDescription))
                .Returns(true);
            _grantedTokenRepositoryFake.Setup(g => g.GetToken(It.IsAny<string>()))
                .Returns(grantedToken);
            _clientRepositoryFake.Setup(c => c.GetClientById(It.IsAny<string>()))
                .Returns(client);

            // ACT
            var result = _getJwsPayload.Execute("access_token");

            // ASSERT
            Assert.IsNotNull(result);
        }

        private void InitializeFakeObjects()
        {
            _grantedTokenValidatorFake = new Mock<IGrantedTokenValidator>();
            _grantedTokenRepositoryFake = new Mock<IGrantedTokenRepository>();
            _jwtParserFake = new Mock<IJwtParser>();
            _clientRepositoryFake = new Mock<IClientRepository>();
            _getJwsPayload = new GetJwsPayload(
                _grantedTokenValidatorFake.Object,
                _grantedTokenRepositoryFake.Object,
                _jwtParserFake.Object,
                _clientRepositoryFake.Object);
        }
    }
}
