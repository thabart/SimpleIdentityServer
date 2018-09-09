using Moq;
using SimpleIdentityServer.Core.Helpers;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.Helpers
{
    public class ResourceOwnerAuthenticateHelperFixture
    {
        private Mock<IAmrHelper> _amrHelperStub;
        private IResourceOwnerAuthenticateHelper _resourceOwnerAuthenticateHelper;

        [Fact]
        public async Task When_Pass_Null_Parameters_Then_Exceptions_Are_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACTS & ASSERTS
            await Assert.ThrowsAsync<ArgumentNullException>(() => _resourceOwnerAuthenticateHelper.Authenticate(null, null, null)).ConfigureAwait(false);
            await Assert.ThrowsAsync<ArgumentNullException>(() => _resourceOwnerAuthenticateHelper.Authenticate("login", null, null)).ConfigureAwait(false);
        }

        private void InitializeFakeObjects()
        {
            _amrHelperStub = new Mock<IAmrHelper>();
            _resourceOwnerAuthenticateHelper = new ResourceOwnerAuthenticateHelper(null, _amrHelperStub.Object);
        }
    }
}
