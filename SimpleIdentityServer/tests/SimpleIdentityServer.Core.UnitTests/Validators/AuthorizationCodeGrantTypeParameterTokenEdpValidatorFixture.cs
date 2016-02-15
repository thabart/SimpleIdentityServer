using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Validators;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.Validators
{
    public sealed class AuthorizationCodeGrantTypeParameterTokenEdpValidatorFixture
    {
        private IAuthorizationCodeGrantTypeParameterTokenEdpValidator _authorizationCodeGrantTypeParameterTokenEdpValidator;

        [Fact]
        public void When_Passing_EmptyCode_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObject();
            var parameter = new AuthorizationCodeGrantTypeParameter();

            // ACT & ASSERTS
            var exception = Assert.Throws<IdentityServerException>(() => _authorizationCodeGrantTypeParameterTokenEdpValidator.Validate(parameter));
            Assert.True(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.MissingParameter, Constants.StandardTokenRequestParameterNames.AuthorizationCodeName));
        }

        [Fact]
        public void When_Passing_Invalid_RedirectUri_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObject();
            var parameter = new AuthorizationCodeGrantTypeParameter
            {
                Code = "code",
                RedirectUri = "redirectUri"
            };

            // ACT & ASSERTS
            var exception = Assert.Throws<IdentityServerException>(() => _authorizationCodeGrantTypeParameterTokenEdpValidator.Validate(parameter));
            Assert.True(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(exception.Message == ErrorDescriptions.TheRedirectionUriIsNotWellFormed);
        }

        private void InitializeFakeObject()
        {
            _authorizationCodeGrantTypeParameterTokenEdpValidator = new AuthorizationCodeGrantTypeParameterTokenEdpValidator();
        }
    }
}
