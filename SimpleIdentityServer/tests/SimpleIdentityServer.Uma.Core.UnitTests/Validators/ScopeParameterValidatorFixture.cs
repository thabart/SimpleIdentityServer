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

using SimpleIdentityServer.Uma.Core.Errors;
using SimpleIdentityServer.Uma.Core.Exceptions;
using SimpleIdentityServer.Uma.Core.Models;
using SimpleIdentityServer.Uma.Core.Validators;
using System;
using Xunit;

namespace SimpleIdentityServer.Uma.Core.UnitTests.Validators
{
    public class ScopeParameterValidatorFixture
    {
        private IScopeParameterValidator _scopeParameterValidator;

        #region Exceptions

        [Fact]
        public void When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(() => _scopeParameterValidator.CheckScopeParameter(null));
        }

        [Fact]
        public void When_Id_Is_Not_Passed_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var scope = new Scope();

            // ACT & ASSERTS
            var exception = Assert.Throws<BaseUmaException>(() => _scopeParameterValidator.CheckScopeParameter(scope));
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheParameterNeedsToBeSpecified, "id"));
        }

        [Fact]
        public void When_Name_Is_Not_Passed_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var scope = new Scope
            {
                Id = "id"
            };

            // ACT & ASSERTS
            var exception = Assert.Throws<BaseUmaException>(() => _scopeParameterValidator.CheckScopeParameter(scope));
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheParameterNeedsToBeSpecified, "name"));
        }

        [Fact]
        public void When_Icon_Uri_Is_Not_Valid_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            const string iconUri = "icon_uri";
            InitializeFakeObjects();
            var scope = new Scope
            {
                Id = "id",
                Name = "name",
                IconUri = iconUri
            };

            // ACT & ASSERTS
            var exception = Assert.Throws<BaseUmaException>(() => _scopeParameterValidator.CheckScopeParameter(scope));
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheUrlIsNotWellFormed, iconUri));

        }

        #endregion

        private void InitializeFakeObjects()
        {
            _scopeParameterValidator = new ScopeParameterValidator();
        }
    }
}
