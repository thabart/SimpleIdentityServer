using System;
using Moq;
using SimpleIdentityServer.Core.Api.Token.Actions;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Repositories;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.Api.Token
{
    public sealed class GetTokenByRefreshTokenGrantTypeActionFixture
    {
        private Mock<IGrantedTokenRepository> _grantedTokenRepositoryFake;

        private Mock<IClientHelper> _clientHelperFake;

        private IGetTokenByRefreshTokenGrantTypeAction _getTokenByRefreshTokenGrantTypeAction;

        [Fact]
        public void When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(() => _getTokenByRefreshTokenGrantTypeAction.Execute(null));
        }

        [Fact]
        public void When_Passing_Invalid_Refresh_Token_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var parameter = new RefreshTokenGrantTypeParameter();
            _grantedTokenRepositoryFake.Setup(g => g.GetTokenByRefreshToken(It.IsAny<string>()))
                .Returns(() => null);

            // ACT & ASSERT
            var ex = Assert.Throws<IdentityServerException>(() => _getTokenByRefreshTokenGrantTypeAction.Execute(parameter));
            Assert.True(ex.Code == ErrorCodes.InvalidGrant);
            Assert.True(ex.Message == ErrorDescriptions.TheRefreshTokenIsNotValid);
        }

        [Fact]
        public void When_Passing_Correct_Parameter_Then_No_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var parameter = new RefreshTokenGrantTypeParameter();
            var grantedToken = new GrantedToken
            {
                IdTokenPayLoad = new JwsPayload()
            };
            _grantedTokenRepositoryFake.Setup(g => g.GetTokenByRefreshToken(It.IsAny<string>()))
                .Returns(grantedToken);

            // ACT & ASSERT
            var ex = Record.Exception(() => _getTokenByRefreshTokenGrantTypeAction.Execute(parameter));
            Assert.Null(ex);
        }

        private void InitializeFakeObjects()
        {
            _grantedTokenRepositoryFake = new Mock<IGrantedTokenRepository>();
            _clientHelperFake = new Mock<IClientHelper>();
            _getTokenByRefreshTokenGrantTypeAction = new GetTokenByRefreshTokenGrantTypeAction(
                _grantedTokenRepositoryFake.Object,
                _clientHelperFake.Object);
        }
    }
}
