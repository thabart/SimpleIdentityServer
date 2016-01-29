using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Validators;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.Validators
{
    public sealed class RefreshTokenGrantTypeParameterValidatorFixture
    {
        private IRefreshTokenGrantTypeParameterValidator _refreshTokenGrantTypeParameterValidator;

        [Fact]
        public void When_Passing_No_Refresh_Token_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var parameter = new RefreshTokenGrantTypeParameter
            {
                RefreshToken = string.Empty
            };

            // ACT  & ASSERT
            var exception = Assert.Throws<IdentityServerException>(() => _refreshTokenGrantTypeParameterValidator.Validate(parameter));
            Assert.True(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.MissingParameter, Constants.StandardTokenRequestParameterNames.RefreshToken));
        }

        [Fact]
        public void When_Passing_Correct_Request_Then_No_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var parameter = new RefreshTokenGrantTypeParameter
            {
                RefreshToken = "refresh_token"
            };

            // ACT  & ASSERT
            var ex = Record.Exception(() => _refreshTokenGrantTypeParameterValidator.Validate(parameter));
            Assert.Null(ex);
        }

        private void InitializeFakeObjects()
        {
            _refreshTokenGrantTypeParameterValidator = new RefreshTokenGrantTypeParameterValidator();
        }
    }
}
