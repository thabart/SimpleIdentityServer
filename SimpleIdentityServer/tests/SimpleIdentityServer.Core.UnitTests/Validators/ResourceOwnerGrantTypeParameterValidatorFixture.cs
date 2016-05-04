#region copyright
// Copyright 2015 Habart Thierry
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

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
