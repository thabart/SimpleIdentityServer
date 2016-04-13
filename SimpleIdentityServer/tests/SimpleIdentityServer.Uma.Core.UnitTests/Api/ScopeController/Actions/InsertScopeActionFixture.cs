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
using SimpleIdentityServer.Uma.Core.Api.ScopeController.Actions;
using SimpleIdentityServer.Uma.Core.Errors;
using SimpleIdentityServer.Uma.Core.Exceptions;
using SimpleIdentityServer.Uma.Core.Models;
using SimpleIdentityServer.Uma.Core.Parameters;
using SimpleIdentityServer.Uma.Core.Repositories;
using SimpleIdentityServer.Uma.Core.Validators;
using System;
using Xunit;

namespace SimpleIdentityServer.Uma.Core.UnitTests.Api.ScopeController.Actions
{
    public class InsertScopeActionFixture
    {
        private Mock<IScopeRepository> _scopeRepositoryStub;

        private Mock<IScopeParameterValidator> _scopeParameterValidator;

        private IInsertScopeAction _insertScopeAction;

        #region Exceptions

        [Fact]
        public void When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(() => _insertScopeAction.Execute(null));
        }

        [Fact]
        public void When_An_Error_Occured_While_Retrieving_Scope_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var addScopeParameter = new AddScopeParameter();
            _scopeRepositoryStub.Setup(s => s.GetScope(It.IsAny<string>()))
                .Callback(() =>
                {
                    throw new Exception();
                });

            // ACT & ASSERTS
            var exception = Assert.Throws<BaseUmaException>(() => _insertScopeAction.Execute(addScopeParameter));
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InternalError);
            Assert.True(exception.Message == ErrorDescriptions.TheScopeCannotBeRetrieved);
        }

        [Fact]
        public void When_Scope_Already_Exists_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string scopeId = "scope_id";
            var addScopeParameter = new AddScopeParameter
            {
                Id = scopeId
            };
            _scopeRepositoryStub.Setup(s => s.GetScope(It.IsAny<string>()))
                .Returns(new Scope());

            // ACT & ASSERTS
            var exception = Assert.Throws<BaseUmaException>(() => _insertScopeAction.Execute(addScopeParameter));
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheScopeAlreadyExists, scopeId));
        }

        [Fact]
        public void When_An_Error_Occured_While_Inserted_Scope_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var addScopeParameter = new AddScopeParameter();
            _scopeRepositoryStub.Setup(s => s.GetScope(It.IsAny<string>()))
                .Returns(() => null);
            _scopeRepositoryStub.Setup(s => s.InsertScope(It.IsAny<Scope>()))
                .Callback(() =>
                {
                    throw new Exception();
                });

            // ACT & ASSERTS
            var exception = Assert.Throws<BaseUmaException>(() => _insertScopeAction.Execute(addScopeParameter));
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InternalError);
            Assert.True(exception.Message == ErrorDescriptions.TheScopeCannotBeInserted);
        }

        #endregion

        #region Happy paths
        
        [Fact]
        public void When_A_Scope_Is_Inserted_Then_True_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            var addScopeParameter = new AddScopeParameter();
            _scopeRepositoryStub.Setup(s => s.GetScope(It.IsAny<string>()))
                .Returns(() => null);
            _scopeRepositoryStub.Setup(s => s.InsertScope(It.IsAny<Scope>()))
                .Returns(new Scope());

            // ACT
            var result = _insertScopeAction.Execute(addScopeParameter);

            // ASSERT
            Assert.True(result);
        }

        #endregion

        private void InitializeFakeObjects()
        {
            _scopeRepositoryStub = new Mock<IScopeRepository>();
            _scopeParameterValidator = new Mock<IScopeParameterValidator>();
            _insertScopeAction = new InsertScopeAction(_scopeRepositoryStub.Object,
                _scopeParameterValidator.Object);
        }
    }
}
