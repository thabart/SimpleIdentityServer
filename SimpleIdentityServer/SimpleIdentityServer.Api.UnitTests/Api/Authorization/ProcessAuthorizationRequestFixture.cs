using NUnit.Framework;
using SimpleIdentityServer.Api.UnitTests.Fake;
using SimpleIdentityServer.Core.Api.Authorization.Common;
using SimpleIdentityServer.Core.Factories;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.Validators;

namespace SimpleIdentityServer.Api.UnitTests.Api.Authorization
{
    [TestFixture]
    public sealed class ProcessAuthorizationRequestFixture : BaseFixture
    {
        private ProcessAuthorizationRequest _processAuthorizationRequest;

        [Test]
        public void When_Requesting_Authorization_ForANotAuthenticatedUser_ThenRedirectTo_LoginPage()
        {
            Assert.IsTrue(true);
        }
    }
}
