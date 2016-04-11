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
using System.Collections.Generic;
using Xunit;

namespace SimpleIdentityServer.Uma.Core.UnitTests.Validators
{
    public class ResourceSetParameterValidatorFixture
    {
        private ResourceSetParameterValidator _resourceSetParameterValidator;

        #region Exceptions

        [Fact]
        public void When_Passing_Null_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERTS
            Assert.Throws<ArgumentNullException>(() => _resourceSetParameterValidator.CheckResourceSetParameter(null));
        }

        [Fact]
        public void When_Name_Is_Not_Pass_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var addResourceParameter = new ResourceSet();

            // ACT & ASSERTS
            var exception = Assert.Throws<BaseUmaException>(() => _resourceSetParameterValidator.CheckResourceSetParameter(addResourceParameter));
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheParameterNeedsToBeSpecified, "name"));
        }

        [Fact]
        public void When_Scopes_Are_Not_Specified_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var addResourceParameter = new ResourceSet
            {
                Name = "name"
            };

            // ACT & ASSERTS
            var exception = Assert.Throws<BaseUmaException>(() => _resourceSetParameterValidator.CheckResourceSetParameter(addResourceParameter));
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheParameterNeedsToBeSpecified, "scopes"));
        }

        [Fact]
        public void When_Icon_Uri_Is_Not_Correct_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string iconUri = "icon_uri";
            var addResourceParameter = new ResourceSet
            {
                Name = "name",
                Scopes = new List<string> { "scope" },
                IconUri = iconUri
            };

            // ACT & ASSERTS
            var exception = Assert.Throws<BaseUmaException>(() => _resourceSetParameterValidator.CheckResourceSetParameter(addResourceParameter));
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheUrlIsNotWellFormed, iconUri));
        }

        [Fact]
        public void When_Uri_Is_Not_Correct_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string uri = "uri";
            var addResourceParameter = new ResourceSet
            {
                Name = "name",
                Scopes = new List<string> { "scope" },
                IconUri = "http://localhost",
                Uri = uri
            };

            // ACT & ASSERTS
            var exception = Assert.Throws<BaseUmaException>(() => _resourceSetParameterValidator.CheckResourceSetParameter(addResourceParameter));
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheUrlIsNotWellFormed, uri));
        }

        #endregion

        private void InitializeFakeObjects()
        {
            _resourceSetParameterValidator = new ResourceSetParameterValidator();
        }
    }
}
