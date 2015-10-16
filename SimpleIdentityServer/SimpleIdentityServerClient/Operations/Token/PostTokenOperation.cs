using System.Net.Http;
using System.Threading.Tasks;

using SimpleIdentityServerClient.DTOs.Request;
using SimpleIdentityServerClient.DTOs.Response;

using SwaggerLibrary;

namespace SimpleIdentityServerClient.Operations.Token
{
    public interface IPostTokenOperation
    {
        Task<GrantedToken> ExecuteAsync(TokenRequest request);
    }

    public class PostTokenOperation : IPostTokenOperation
    {
        private readonly ISwaggerDocumentationParser _swaggerDocumentationParser;

        private readonly IWebApiRequestor _webApiRequestor;

        public PostTokenOperation(
            ISwaggerDocumentationParser swaggerDocumentationParser,
            IWebApiRequestor webApiRequestor
            )
        {
            _swaggerDocumentationParser = swaggerDocumentationParser;
            _webApiRequestor = webApiRequestor;
        }

        public async Task<GrantedToken> ExecuteAsync(TokenRequest request)
        {
            var contract = await _swaggerDocumentationParser.GetContract();
            var response = await _webApiRequestor
                .CreateRequest("PostToken", contract)
                .PassParameters(request)
                .ExecuteRequest();
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsAsync<GrantedToken>();
        }
    }
}
