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


        public void StartAuthorizationCodeFlow(string clientId, string scope, string individualClaims)
        {
        }

        public void StartProcessingAuthorizationRequest(string jsonAuthorizationRequest)
        {
        }

        public void EndProcessingAuthorizationRequest(string jsonAuthorizationRequest, string actionType, string actionName)
        {
        }
        
        public void EndAuthorization(string actionType, string controllerAction, string parameters)
        {
        }


        public void StartGeneratingAuthorizationResponseToClient(string clientId, string responseTypes)
        {
        }

        public void GrantAccessToClient(string clientId, string accessToken, string scopes)
        {
        }

        public void GrantAuthorizationCodeToClient(string clientId, string authorizationCode, string scopes)
        {
        }

        public void EndGeneratingAuthorizationResponseToClient(string clientId, string parameters)
        {
        }


        public void EndAuthorizationCodeFlow(string clientId, string actionType, string actionName)
        {
        }


        public void StartImplicitFlow(string clientId, string scope, string individualClaims)
        {
        }

        public void EndImplicitFlow(string clientId, string actionType, string actionName)
        {
        }
    }
}
