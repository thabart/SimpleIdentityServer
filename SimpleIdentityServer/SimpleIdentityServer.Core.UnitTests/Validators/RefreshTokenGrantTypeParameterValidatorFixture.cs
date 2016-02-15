using NUnit.Framework;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Validators;

namespace SimpleIdentityServer.Core.UnitTests.Validators
{
    [TestFixture]
    public sealed class RefreshTokenGrantTypeParameterValidatorFixture
    {
        private IRefreshTokenGrantTypeParameterValidator _refreshTokenGrantTypeParameterValidator;

        [Test]
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
            Assert.IsTrue(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.IsTrue(exception.Message == string.Format(ErrorDescriptions.MissingParameter, Constants.StandardTokenRequestParameterNames.RefreshToken));
        }

        [Test]
        public void When_Passing_Correct_Request_Then_No_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var parameter = new RefreshTokenGrantTypeParameter
            {
                RefreshToken = "refresh_token"
            };

            // ACT  & ASSERT
            Assert.DoesNotThrow(() => _refreshTokenGrantTypeParameterValidator.Validate(parameter));
        }

        private void InitializeFakeObjects()
        {
            _refreshTokenGrantTypeParameterValidator = new RefreshTokenGrantTypeParameterValidator();
        }
    }
}
