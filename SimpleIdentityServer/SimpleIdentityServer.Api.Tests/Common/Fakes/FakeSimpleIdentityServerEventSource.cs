using SimpleIdentityServer.Logging;

namespace SimpleIdentityServer.Api.Tests.Common.Fakes
{
    public class FakeSimpleIdentityServerEventSource : ISimpleIdentityServerEventSource
    {
        public void StartAuthorization(string clientId, string responseType, string scope, string individualClaims)
        {
        }

        public void EndAuthorization(string actionType, string controllerAction)
        {
        }

        public void OpenIdFailure(string code, string description, string state)
        {
        }
    }
}
