using NUnit.Framework;
using SimpleIdentityServer.Core;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Validators;

namespace SimpleIdentityServer.Core.UnitTests.Validators
{
    [TestFixture]
    public sealed class ResourceOwnerGrantTypeParameterValidatorFixture
    {
        private IResourceOwnerGrantTypeParameterValidator _resourceOwnerGrantTypeParameterValidator;

        [Test]
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
            Assert.IsTrue(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.IsTrue(exception.Message == string.Format(ErrorDescriptions.MissingParameter, Constants.StandardTokenRequestParameterNames.ClientIdName));
        }

        [Test]
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
            Assert.IsTrue(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.IsTrue(exception.Message == string.Format(ErrorDescriptions.MissingParameter, Constants.StandardTokenRequestParameterNames.UserName));
        }
        
        [Test]
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
            Assert.IsTrue(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.IsTrue(exception.Message == string.Format(ErrorDescriptions.MissingParameter, Constants.StandardTokenRequestParameterNames.PasswordName));
        }

        private void InitializeFakeObject()
        {
            _resourceOwnerGrantTypeParameterValidator = new ResourceOwnerGrantTypeParameterValidator();
        }
    }
}
