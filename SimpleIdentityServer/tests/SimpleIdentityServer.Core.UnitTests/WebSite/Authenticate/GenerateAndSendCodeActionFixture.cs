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
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Core.Services;
using SimpleIdentityServer.Core.WebSite.Authenticate.Actions;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.WebSite.Authenticate
{
    public class GenerateAndSendCodeActionFixture
    {
        private Mock<IResourceOwnerRepository> _resourceOwnerRepositoryStub;
        private Mock<IConfirmationCodeRepository> _confirmationCodeRepositoryStub;
        private Mock<ITwoFactorAuthenticationHandler> _twoFactorAuthenticationHandlerStub;
        private Mock<IAuthenticateResourceOwnerService> _authenticateResourceOwnerServiceStub;
        private IGenerateAndSendCodeAction _generateAndSendCodeAction;

        [Fact]
        public async Task When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            await Assert.ThrowsAsync<ArgumentNullException>(() => _generateAndSendCodeAction.ExecuteAsync(null));
        }

        [Fact]
        public async Task When_ResourceOwner_Doesnt_Exist_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            _authenticateResourceOwnerServiceStub.Setup(r => r.AuthenticateResourceOwnerAsync(It.IsAny<string>()))
                .Returns(Task.FromResult((ResourceOwner)null));

            // ACT & ASSERTS
            var exception = await Assert.ThrowsAsync<IdentityServerException>(() => _generateAndSendCodeAction.ExecuteAsync("subject"));
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.UnhandledExceptionCode);
            Assert.True(exception.Message == ErrorDescriptions.TheRoDoesntExist);
        }

        [Fact]
        public async Task When_Two_Factor_Auth_Is_Not_Enabled_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            _authenticateResourceOwnerServiceStub.Setup(r => r.AuthenticateResourceOwnerAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(new ResourceOwner
                {
                    TwoFactorAuthentication = TwoFactorAuthentications.NONE
                }));

            // ACT & ASSERTS
            var exception = await Assert.ThrowsAsync<IdentityServerException>(() => _generateAndSendCodeAction.ExecuteAsync("subject"));
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.UnhandledExceptionCode);
            Assert.True(exception.Message == ErrorDescriptions.TwoFactorAuthenticationIsNotEnabled);
        }

        [Fact]
        public async Task When_Code_Cannot_Be_Inserted_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            _authenticateResourceOwnerServiceStub.Setup(r => r.AuthenticateResourceOwnerAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(new ResourceOwner
                {
                    TwoFactorAuthentication = TwoFactorAuthentications.Email
                }));
            _confirmationCodeRepositoryStub.Setup(r => r.GetAsync(It.IsAny<string>())).Returns(Task.FromResult((ConfirmationCode)null));
            _confirmationCodeRepositoryStub.Setup(r => r.AddAsync(It.IsAny<ConfirmationCode>()))
                .Returns(Task.FromResult(false));

            // ACT & ASSERTS
            var exception = await Assert.ThrowsAsync<IdentityServerException>(() => _generateAndSendCodeAction.ExecuteAsync("subject"));
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.UnhandledExceptionCode);
            Assert.True(exception.Message == ErrorDescriptions.TheConfirmationCodeCannotBeSaved);
        }

        [Fact]
        public async Task When_Code_Is_Generated_And_Inserted_Then_Handler_Is_Called()
        {
            // ARRANGE
            InitializeFakeObjects();
            _authenticateResourceOwnerServiceStub.Setup(r => r.AuthenticateResourceOwnerAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(new ResourceOwner
                {
                    TwoFactorAuthentication = TwoFactorAuthentications.Email
                }));
            _confirmationCodeRepositoryStub.Setup(r => r.GetAsync(It.IsAny<string>())).Returns(Task.FromResult((ConfirmationCode)null));
            _confirmationCodeRepositoryStub.Setup(r => r.AddAsync(It.IsAny<ConfirmationCode>()))
                .Returns(Task.FromResult(true));

            // ACT
            await _generateAndSendCodeAction.ExecuteAsync("subject");

            // ASSERTS
            _twoFactorAuthenticationHandlerStub.Verify(t => t.SendCode(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<ResourceOwner>()));
        }

        private void InitializeFakeObjects()
        {
            _resourceOwnerRepositoryStub = new Mock<IResourceOwnerRepository>();
            _confirmationCodeRepositoryStub = new Mock<IConfirmationCodeRepository>();
            _twoFactorAuthenticationHandlerStub = new Mock<ITwoFactorAuthenticationHandler>();
            _authenticateResourceOwnerServiceStub = new Mock<IAuthenticateResourceOwnerService>();
            _generateAndSendCodeAction = new GenerateAndSendCodeAction(
                _resourceOwnerRepositoryStub.Object,
                _confirmationCodeRepositoryStub.Object,
                _twoFactorAuthenticationHandlerStub.Object,
                _authenticateResourceOwnerServiceStub.Object);
        }
    }
}
