using Xunit;

namespace SimpleIdentityServer.Host.Tests.Apis
{
    public class ProfileClientFixture : IClassFixture<TestOauthServerFixture>
    {
        const string baseUrl = "http://localhost:5000";
        private readonly TestOauthServerFixture _server;

        public ProfileClientFixture(TestOauthServerFixture server)
        {
            _server = server;
        }

        #region Errors

        #region Add profile



        #endregion

        #endregion

        private void InitializeFakeObjects()
        {

        }
    }
}
