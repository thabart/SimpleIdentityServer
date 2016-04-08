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

using Moq;
using SimpleIdentityServer.Uma.Core.Api.Parameters;
using SimpleIdentityServer.Uma.Core.Api.ResourceSetController.Actions;
using SimpleIdentityServer.Uma.Core.Errors;
using SimpleIdentityServer.Uma.Core.Exceptions;
using SimpleIdentityServer.Uma.Core.Models;
using SimpleIdentityServer.Uma.Core.Repositories;
using System;
using System.Collections.Generic;
using Xunit;

namespace SimpleIdentityServer.Uma.Core.UnitTests.Api.ResourceSetController.Actions
{
    public class AddResourceSetActionFixture
    {
        private Mock<IResourceSetRepository> _resourceSetRepositoryStub;

        private IAddResourceSetAction _addResourceSetAction;

        #region Exceptions

        [Fact]
        public void When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(() => _addResourceSetAction.Execute(null));
        }

        [Fact]
        public void When_Name_Is_Not_Pass_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var addResourceParameter = new AddResouceSetParameter();

            // ACT & ASSERTS
            var exception = Assert.Throws<BaseUmaException>(() => _addResourceSetAction.Execute(addResourceParameter));
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheParameterNeedsToBeSpecified, "name"));
        }

        [Fact]
        public void When_Scopes_Are_Not_Specified_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var addResourceParameter = new AddResouceSetParameter
            {
                Name = "name"
            };

            // ACT & ASSERTS
            var exception = Assert.Throws<BaseUmaException>(() => _addResourceSetAction.Execute(addResourceParameter));
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
            var addResourceParameter = new AddResouceSetParameter
            {
                Name = "name",
                Scopes = new List<string> { "scope" },
                IconUri = iconUri
            };

            // ACT & ASSERTS
            var exception = Assert.Throws<BaseUmaException>(() => _addResourceSetAction.Execute(addResourceParameter));
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
            var addResourceParameter = new AddResouceSetParameter
            {
                Name = "name",
                Scopes = new List<string> { "scope" },
                IconUri = "http://localhost",
                Uri = uri
            };

            // ACT & ASSERTS
            var exception = Assert.Throws<BaseUmaException>(() => _addResourceSetAction.Execute(addResourceParameter));
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheUrlIsNotWellFormed, uri));
        }

        [Fact]
        public void When_Resource_Set_Cannot_Be_Inserted_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var addResourceParameter = new AddResouceSetParameter
            {
                Name = "name",
                Scopes = new List<string> { "scope" },
                IconUri = "http://localhost",
                Uri = "http://localhost"
            };
            _resourceSetRepositoryStub.Setup(r => r.Insert(It.IsAny<ResourceSet>()))
                .Returns(() => null);

            // ACT & ASSERTS
            var exception = Assert.Throws<BaseUmaException>(() => _addResourceSetAction.Execute(addResourceParameter));
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InternalError);
            Assert.True(exception.Message == ErrorDescriptions.TheResourceSetCannotBeInserted);
        }

        #endregion

        #region Happy path

        [Fact]
        public void When_ResourceSet_Is_Inserted_Then_Id_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string id = "id";
            var addResourceParameter = new AddResouceSetParameter
            {
                Name = "name",
                Scopes = new List<string> { "scope" },
                IconUri = "http://localhost",
                Uri = "http://localhost"
            };
            _resourceSetRepositoryStub.Setup(r => r.Insert(It.IsAny<ResourceSet>()))
                .Returns(new ResourceSet { Id = id });

            // ACT
            var result = _addResourceSetAction.Execute(addResourceParameter);

            // ASSERTS
            Assert.NotNull(result);
            Assert.True(result == id);
        }

        #endregion

        private void InitializeFakeObjects()
        {
            _resourceSetRepositoryStub = new Mock<IResourceSetRepository>();
            _addResourceSetAction = new AddResourceSetAction(_resourceSetRepositoryStub.Object);
        }
    }
}
