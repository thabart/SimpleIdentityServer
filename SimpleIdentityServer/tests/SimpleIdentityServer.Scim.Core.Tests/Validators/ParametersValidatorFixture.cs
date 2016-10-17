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
// limitations under the License.C:\Projects\SimpleIdentityServer\SimpleIdentityServer\src\SimpleIdentityServer.Scim.Core\DTOs\
#endregion

using SimpleIdentityServer.Scim.Core.Errors;
using SimpleIdentityServer.Scim.Core.Validators;
using System;
using Xunit;

namespace SimpleIdentityServer.Scim.Core.Tests.Validators
{
    public class ParametersValidatorFixture
    {
        private IParametersValidator _parametersValidator;

        [Fact]
        public void When_Passing_Null_Or_Empty_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObject();

            // ACTS & ASSERTS
            Assert.Throws<ArgumentNullException>(() => _parametersValidator.ValidateLocationPattern(null));
            Assert.Throws<ArgumentNullException>(() => _parametersValidator.ValidateLocationPattern(string.Empty));
        }

        [Fact]
        public void When_Parameter_Is_Not_Correct_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            const string locationPattern = "invalid_location_pattern";
            InitializeFakeObject();

            // ACT & ASSERT
            var exception = Assert.Throws<ArgumentException>(() => _parametersValidator.ValidateLocationPattern("invalid_location_pattern"));
            Assert.NotNull(exception);
            Assert.True(exception.Message == string.Format(ErrorMessages.TheLocationPatternIsNotCorrect, locationPattern));
        }

        private void InitializeFakeObject()
        {
            _parametersValidator = new ParametersValidator();
        }
    }
}
