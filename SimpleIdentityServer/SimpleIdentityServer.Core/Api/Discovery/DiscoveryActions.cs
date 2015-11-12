using SimpleIdentityServer.Core.Api.Discovery.Actions;
using SimpleIdentityServer.Core.Results;

namespace SimpleIdentityServer.Core.Api.Discovery
{
    public interface IDiscoveryActions
    {
        DiscoveryInformation CreateDiscoveryInformation();
    }

    public class DiscoveryActions : IDiscoveryActions
    {
        private readonly ICreateDiscoveryDocumentationAction _createDiscoveryDocumentationAction;

        public DiscoveryActions(ICreateDiscoveryDocumentationAction createDiscoveryDocumentationAction)
        {
            _createDiscoveryDocumentationAction = createDiscoveryDocumentationAction;
        }

        public DiscoveryInformation CreateDiscoveryInformation()
        {
            return _createDiscoveryDocumentationAction.Execute();
        }
    }
}
