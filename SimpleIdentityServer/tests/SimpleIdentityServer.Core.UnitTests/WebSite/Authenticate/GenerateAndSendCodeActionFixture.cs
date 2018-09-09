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
using SimpleIdentityServer.Core.Common.Models;
using SimpleIdentityServer.Core.Common.Repositories;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Services;
using SimpleIdentityServer.Core.WebSite.Authenticate.Actions;
using SimpleIdentityServer.Store;
using SimpleIdentityServer.TwoFactorAuthentication;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.WebSite.Authenticate
{
    public class GenerateAndSendCodeActionFixture
    {
        private Mock<IResourceOwnerRepository> _resourceOwnerRepositoryStub;
        private Mock<IConfirmationCodeStore> _confirmationCodeStoreStub;
        private Mock<ITwoFactorAuthenticationHandler> _twoFactorAuthenticationHandlerStub;
        private IGenerateAndSendCodeAction _generateAndSendCodeAction;

        [Fact]
        public async Task When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            await Assert.ThrowsAsync<ArgumentNullException>(() => _generateAndSendCodeAction.ExecuteAsync(null)).ConfigureAwait(false);
        }

        [Fact]
        public async Task When_ResourceOwner_Doesnt_Exist_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            _resourceOwnerRepositoryStub.Setup(r => r.GetAsync(It.IsAny<string>()))
                .Returns(Task.FromResult((ResourceOwner)null));

            // ACT & ASSERTS
            var exception = await Assert.ThrowsAsync<IdentityServerException>(() => _generateAndSendCodeAction.ExecuteAsync("subject")).ConfigureAwait(false);
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.UnhandledExceptionCode);
            Assert.True(exception.Message == ErrorDescriptions.TheRoDoesntExist);
        }

        [Fact]
        public async Task When_Two_Factor_Auth_Is_Not_Enabled_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            _resourceOwnerRepositoryStub.Setup(r => r.GetAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(new ResourceOwner
                {
                    TwoFactorAuthentication = string.Empty
                }));

            // ACT & ASSERTS
            var exception = await Assert.ThrowsAsync<IdentityServerException>(() => _generateAndSendCodeAction.ExecuteAsync("subject")).ConfigureAwait(false);
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.UnhandledExceptionCode);
            Assert.True(exception.Message == ErrorDescriptions.TwoFactorAuthenticationIsNotEnabled);
        }

        [Fact]
        public async Task When_ResourceOwner_Doesnt_Have_The_Required_Claim_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            _resourceOwnerRepositoryStub.Setup(r => r.GetAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(new ResourceOwner
                {
                    TwoFactorAuthentication = "email",
                    Claims = new List<Claim>
                    {
                        new Claim("key", "value")
                    },
                    Id = "subject"
                }));
            var fakeAuthService = new Mock<ITwoFactorAuthenticationService>();
            fakeAuthService.SetupGet(f => f.RequiredClaim).Returns("claim");
            _twoFactorAuthenticationHandlerStub.Setup(t => t.Get(It.IsAny<string>())).Returns(fakeAuthService.Object);

            // ACT
            var exception = await Assert.ThrowsAsync<ClaimRequiredException>(() => _generateAndSendCodeAction.ExecuteAsync("subject")).ConfigureAwait(false);

            // ASSERT
            Assert.NotNull(exception);
        }

        [Fact]
        public async Task When_Code_Cannot_Be_Inserted_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            _resourceOwnerRepositoryStub.Setup(r => r.GetAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(new ResourceOwner
                {
                    TwoFactorAuthentication = "email",
                    Claims = new List<Claim>
                    {
                        new Claim("key", "value")
                    },
                    Id = "subject"
                }));
            var fakeAuthService = new Mock<ITwoFactorAuthenticationService>();
            fakeAuthService.SetupGet(f => f.RequiredClaim).Returns("key");
            _twoFactorAuthenticationHandlerStub.Setup(t => t.Get(It.IsAny<string>())).Returns(fakeAuthService.Object);
            _confirmationCodeStoreStub.Setup(r => r.Get(It.IsAny<string>())).Returns(Task.FromResult((ConfirmationCode)null));
            _confirmationCodeStoreStub.Setup(r => r.Add(It.IsAny<ConfirmationCode>()))
                .Returns(Task.FromResult(false));

            // ACT
            var exception = await Assert.ThrowsAsync<IdentityServerException>(() => _generateAndSendCodeAction.ExecuteAsync("subject")).ConfigureAwait(false);

            // ASSERTS
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.UnhandledExceptionCode);
            Assert.True(exception.Message == ErrorDescriptions.TheConfirmationCodeCannotBeSaved);
        }

        [Fact]
        public async Task When_Code_Is_Generated_And_Inserted_Then_Handler_Is_Called()
        {
            // ARRANGE
            InitializeFakeObjects();
            _resourceOwnerRepositoryStub.Setup(r => r.GetAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(new ResourceOwner
                {
                    TwoFactorAuthentication = "email",
                    Claims = new List<Claim>
                    {
                        new Claim("key", "value")
                    },
                    Id = "subject"
                }));
            var fakeAuthService = new Mock<ITwoFactorAuthenticationService>();
            fakeAuthService.SetupGet(f => f.RequiredClaim).Returns("key");
            _twoFactorAuthenticationHandlerStub.Setup(t => t.Get(It.IsAny<string>())).Returns(fakeAuthService.Object);
            _confirmationCodeStoreStub.Setup(r => r.Get(It.IsAny<string>())).Returns(Task.FromResult((ConfirmationCode)null));
            _confirmationCodeStoreStub.Setup(r => r.Add(It.IsAny<ConfirmationCode>()))
                .Returns(Task.FromResult(true));

            // ACT
            await _generateAndSendCodeAction.ExecuteAsync("subject").ConfigureAwait(false);

            // ASSERTS
            _twoFactorAuthenticationHandlerStub.Verify(t => t.SendCode(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ResourceOwner>()));
        }

        private void InitializeFakeObjects()
        {
            _resourceOwnerRepositoryStub = new Mock<IResourceOwnerRepository>();
            _confirmationCodeStoreStub = new Mock<IConfirmationCodeStore>();
            _twoFactorAuthenticationHandlerStub = new Mock<ITwoFactorAuthenticationHandler>();
            _generateAndSendCodeAction = new GenerateAndSendCodeAction(
                _resourceOwnerRepositoryStub.Object,
                _confirmationCodeStoreStub.Object,
                _twoFactorAuthenticationHandlerStub.Object);
        }
    }
}
