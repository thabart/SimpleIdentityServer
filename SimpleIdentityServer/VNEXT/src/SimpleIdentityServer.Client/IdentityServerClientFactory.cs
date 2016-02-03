using SimpleIdentityServer.Client.Operations.Token;

using SwaggerLibrary;
using SwaggerLibrary.Helpers;

namespace SimpleIdentityServer.Client
{
    public interface IIdentityServerClientFactory
    {
        IIdentityServerClient CreateClient(string url);
    }

    public class IdentityServerClientFactory
    {
        public IIdentityServerClient CreateClient(string documentationUrl, string host)
        {
            var httpClientHelper = new HttpClientHelper();
            var webApiRequestor = new WebApiRequestor(host, httpClientHelper);
            var swaggerDocumentationParser = new SwaggerDocumentationParser(documentationUrl, httpClientHelper);
            var postTokenOperation = new PostTokenOperation(swaggerDocumentationParser, webApiRequestor);
            return new IdentityServerClient(postTokenOperation);
        }
    }
}
