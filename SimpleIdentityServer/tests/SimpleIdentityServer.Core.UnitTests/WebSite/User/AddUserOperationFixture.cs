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
using SimpleIdentityServer.AccountFilter;
using SimpleIdentityServer.Core.Api.Profile.Actions;
using SimpleIdentityServer.Core.Common.Models;
using SimpleIdentityServer.Core.Common.Repositories;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Services;
using SimpleIdentityServer.Core.WebSite.User.Actions;
using SimpleIdentityServer.OpenId.Logging;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.WebSite.User
{
    public class AddUserOperationFixture
    {
        private Mock<IResourceOwnerRepository> _resourceOwnerRepositoryStub;
        private Mock<IClaimRepository> _claimsRepositoryStub;
        private Mock<ILinkProfileAction> _linkProfileActionStub;
        private Mock<IOpenIdEventSource> _openidEventSourceStub;
        private Mock<ISubjectBuilder> _subjectBuilderStub;
        private Mock<IAccountFilter> _accountFilterStub;
        private Mock<IUserClaimsEnricher> _userClaimsEnricherStub;
        private IAddUserOperation _addResourceOwnerAction;
        
        [Fact]
        public async Task When_Passing_Null_Parameters_Then_Exceptions_Are_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACTS & ASSERTS
            await Assert.ThrowsAsync<ArgumentNullException>(() => _addResourceOwnerAction.Execute(null, null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => _addResourceOwnerAction.Execute(new AddUserParameter(null), null));
        }

        [Fact]
        public async Task When_ResourceOwner_With_Same_Credentials_Exists_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var parameter = new AddUserParameter("password");

            _resourceOwnerRepositoryStub.Setup(r => r.GetAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(new ResourceOwner()));

            // ACT
            var exception = await Assert.ThrowsAsync<IdentityServerException>(() => _addResourceOwnerAction.Execute(parameter, null));

            // ASSERTS
            Assert.NotNull(exception);
            Assert.True(exception.Code == Errors.ErrorCodes.UnhandledExceptionCode);
            Assert.True(exception.Message == Errors.ErrorDescriptions.TheRoWithCredentialsAlreadyExists);
        }

        [Fact]
        public async Task When_ResourceOwner_Cannot_Be_Added_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            _subjectBuilderStub.Setup(s => s.BuildSubject()).Returns(Task.FromResult("sub")); _accountFilterStub.Setup(s => s.Check(It.IsAny<IEnumerable<Claim>>())).Returns(Task.FromResult(new AccountFilterResult
            {
                IsValid = true
            }));
            _resourceOwnerRepositoryStub.Setup(r => r.InsertAsync(It.IsAny<ResourceOwner>())).Returns(Task.FromResult(false));
            var parameter = new AddUserParameter("password");

            // ACT
            var exception = await Assert.ThrowsAsync<IdentityServerException>(() => _addResourceOwnerAction.Execute(parameter, null));

            // ASSERTS
            Assert.NotNull(exception);
            Assert.Equal("unhandled_exception", exception.Code);
            Assert.Equal("An error occured while trying to insert the resource owner", exception.Message);
        }
        
        [Fact]
        public async Task When_Add_ResourceOwner_Then_Operation_Is_Called()
        {
            // ARRANGE
            InitializeFakeObjects();
            var parameter = new AddUserParameter("password", new List<Claim>());
            _accountFilterStub.Setup(s => s.Check(It.IsAny<IEnumerable<Claim>>())).Returns(Task.FromResult(new AccountFilterResult
            {
                IsValid = true
            }));
            _subjectBuilderStub.Setup(s => s.BuildSubject()).Returns(Task.FromResult("sub"));
            _resourceOwnerRepositoryStub.Setup(r => r.GetAsync(It.IsAny<string>()))
                .Returns(Task.FromResult((ResourceOwner)null));
            _resourceOwnerRepositoryStub.Setup(r => r.InsertAsync(It.IsAny<ResourceOwner>())).Returns(Task.FromResult(true));

            // ACT
            await _addResourceOwnerAction.Execute(parameter, null);

            // ASSERT
            _resourceOwnerRepositoryStub.Verify(r => r.InsertAsync(It.IsAny<ResourceOwner>()));
            _openidEventSourceStub.Verify(o => o.AddResourceOwner("sub"));
        }

        private void InitializeFakeObjects()
        {
            _resourceOwnerRepositoryStub = new Mock<IResourceOwnerRepository>();
            _claimsRepositoryStub = new Mock<IClaimRepository>();
            _linkProfileActionStub = new Mock<ILinkProfileAction>();
            _openidEventSourceStub = new Mock<IOpenIdEventSource>();
            _subjectBuilderStub = new Mock<ISubjectBuilder>();
            _accountFilterStub = new Mock<IAccountFilter>();
            _userClaimsEnricherStub = new Mock<IUserClaimsEnricher>();
            _addResourceOwnerAction = new AddUserOperation(
                _resourceOwnerRepositoryStub.Object,
                _claimsRepositoryStub.Object,
                _linkProfileActionStub.Object,
                _accountFilterStub.Object,
                _openidEventSourceStub.Object,
                new List<IUserClaimsEnricher> { _userClaimsEnricherStub.Object },
                _subjectBuilderStub.Object);
        }
    }
}
