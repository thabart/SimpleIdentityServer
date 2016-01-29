using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Validators;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.Validators
{
    public sealed class ResourceOwnerGrantTypeParameterValidatorFixture
    {
        private IResourceOwnerGrantTypeParameterValidator _resourceOwnerGrantTypeParameterValidator;

        [Fact]
        public void When_Passing_EmptyClientId_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObject();
            var parameter = new ResourceOwnerGrantTypeParameter
            {
                ClientId = null
            };

            // ACT & ASSERT
            var exception = Assert.Throws<IdentityServerException>(() => _resourceOwnerGrantTypeParameterValidator.Validate(parameter));
            Assert.True(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.MissingParameter, Constants.StandardTokenRequestParameterNames.ClientIdName));
        }

        [Fact]
        public void When_Passing_Empty_UserName_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObject();
            var parameter = new ResourceOwnerGrantTypeParameter
            {
                ClientId = "clientId",
                UserName = null
            };

            // ACT & ASSERT
            var exception = Assert.Throws<IdentityServerException>(() => _resourceOwnerGrantTypeParameterValidator.Validate(parameter));
            Assert.True(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.MissingParameter, Constants.StandardTokenRequestParameterNames.UserName));
        }

        [Fact]
        public void When_Passing_Empty_Password_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObject();
            var parameter = new ResourceOwnerGrantTypeParameter
            {
                ClientId = "clientId",
                UserName = "userName",
                Password = null
            };

            // ACT & ASSERT
            var exception = Assert.Throws<IdentityServerException>(() => _resourceOwnerGrantTypeParameterValidator.Validate(parameter));
            Assert.True(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.MissingParameter, Constants.StandardTokenRequestParameterNames.PasswordName));
        }

        private void InitializeFakeObject()
        {
            _resourceOwnerGrantTypeParameterValidator = new ResourceOwnerGrantTypeParameterValidator();
        }
    }
}
