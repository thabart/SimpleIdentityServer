using NUnit.Framework;
using SimpleIdentityServer.Core;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Validators;

namespace SimpleIdentityServer.Api.UnitTests.Validators
{
    [TestFixture]
    public sealed class AuthorizationCodeGrantTypeParameterTokenEdpValidatorFixture
    {
        private IAuthorizationCodeGrantTypeParameterTokenEdpValidator _authorizationCodeGrantTypeParameterTokenEdpValidator;

        [Test]
        public void When_Passing_EmptyCode_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObject();
            var parameter = new AuthorizationCodeGrantTypeParameter();

            // ACT & ASSERTS
            var exception = Assert.Throws<IdentityServerException>(() => _authorizationCodeGrantTypeParameterTokenEdpValidator.Validate(parameter));
            Assert.IsTrue(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.IsTrue(exception.Message == string.Format(ErrorDescriptions.MissingParameter, Constants.StandardTokenRequestParameterNames.AuthorizationCodeName));
        }

        [Test]
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
            Assert.IsTrue(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.IsTrue(exception.Message == ErrorDescriptions.TheRedirectionUriIsNotWellFormed);
        }

        private void InitializeFakeObject()
        {
            _authorizationCodeGrantTypeParameterTokenEdpValidator = new AuthorizationCodeGrantTypeParameterTokenEdpValidator();
        }
    }
}
