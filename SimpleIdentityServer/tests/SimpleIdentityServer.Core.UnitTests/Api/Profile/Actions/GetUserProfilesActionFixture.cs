﻿using Moq;
using SimpleIdentityServer.Core.Api.Profile.Actions;
using SimpleIdentityServer.Core.Common.Models;
using SimpleIdentityServer.Core.Common.Parameters;
using SimpleIdentityServer.Core.Common.Repositories;
using SimpleIdentityServer.Core.Exceptions;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.Api.Profile.Actions
{
    public class GetUserProfilesActionFixture
    {
        private Mock<IProfileRepository> _profileRepositoryStub;
        private Mock<IResourceOwnerRepository> _resourceOwnerRepositoryStub;
        private IGetUserProfilesAction _getProfileAction;

        [Fact]
        public async Task WhenPassingNullParameterThenExceptionIsThrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACTS & ASSERTS
            await Assert.ThrowsAsync<ArgumentNullException>(() => _getProfileAction.Execute(null)).ConfigureAwait(false);
            await Assert.ThrowsAsync<ArgumentNullException>(() => _getProfileAction.Execute(string.Empty)).ConfigureAwait(false);
        }

        [Fact]
        public async Task When_User_Doesnt_Exist_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            
            // ACT & ASSERT
            var ex = await Assert.ThrowsAsync<IdentityServerException>(() => _getProfileAction.Execute("subject")).ConfigureAwait(false);
            Assert.NotNull(ex);
            Assert.Equal("internal_error", ex.Code);
            Assert.Equal("the resource owner doesn't exist", ex.Message);
        }

        [Fact]
        public async Task WhenGetProfileThenOperationIsCalled()
        {
            const string subject = "subject";
            // ARRANGE
            InitializeFakeObjects();
            _resourceOwnerRepositoryStub.Setup(r => r.GetAsync(It.IsAny<string>())).Returns(Task.FromResult(new ResourceOwner()));

            // ACT
            await _getProfileAction.Execute(subject).ConfigureAwait(false);

            // ASSERT
            _profileRepositoryStub.Verify(p => p.Search(It.Is<SearchProfileParameter>(r => r.ResourceOwnerIds.Contains(subject))));
        }

        private void InitializeFakeObjects()
        {
            _profileRepositoryStub = new Mock<IProfileRepository>();
            _resourceOwnerRepositoryStub = new Mock<IResourceOwnerRepository>();
            _getProfileAction = new GetUserProfilesAction(_profileRepositoryStub.Object, _resourceOwnerRepositoryStub.Object);
        }
    }
}
