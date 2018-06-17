using Moq;
using SimpleIdentityServer.Core.Api.Profile.Actions;
using SimpleIdentityServer.Core.Common.Parameters;
using SimpleIdentityServer.Core.Common.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.Api.Profile.Actions
{
    public class GetUserProfilesActionFixture
    {
        private Mock<IProfileRepository> _profileRepositoryStub;
        private IGetUserProfilesAction _getProfileAction;

        [Fact]
        public async Task WhenPassingNullParameterThenExceptionIsThrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACTS & ASSERTS
            await Assert.ThrowsAsync<ArgumentNullException>(() => _getProfileAction.Execute(null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => _getProfileAction.Execute(string.Empty));
        }

        [Fact]
        public async Task WhenGetProfileThenOperationIsCalled()
        {
            const string subject = "subject";
            // ARRANGE
            InitializeFakeObjects();

            // ACT
            await _getProfileAction.Execute(subject);

            // ASSERT
            _profileRepositoryStub.Verify(p => p.Search(It.Is<SearchProfileParameter>(r => r.ResourceOwnerIds.Contains(subject))));
        }

        private void InitializeFakeObjects()
        {
            _profileRepositoryStub = new Mock<IProfileRepository>();
            _getProfileAction = new GetUserProfilesAction(_profileRepositoryStub.Object);
        }
    }
}
