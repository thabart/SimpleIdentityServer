using SimpleIdentityServer.Core.Common.Models;
using SimpleIdentityServer.Uma.Core.Api.Token.Actions;
using SimpleIdentityServer.Uma.Core.Parameters;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Uma.Core.Api.Token
{
    public interface IUmaTokenActions
    {
        Task<GrantedToken> GetTokenByTicketId(GetTokenViaTicketIdParameter parameter, AuthenticationHeaderValue authenticationHeaderValue, X509Certificate2 certificate = null);
    }

    internal sealed class UmaTokenActions : IUmaTokenActions
    {
        private readonly IGetTokenByTicketIdAction _getTokenByTicketIdAction;

        public UmaTokenActions(IGetTokenByTicketIdAction getTokenByTicketIdAction)
        {
            _getTokenByTicketIdAction = getTokenByTicketIdAction;
        }

        public Task<GrantedToken> GetTokenByTicketId(GetTokenViaTicketIdParameter parameter, AuthenticationHeaderValue authenticationHeaderValue, X509Certificate2 certificate = null)
        {
            return _getTokenByTicketIdAction.Execute(parameter, authenticationHeaderValue, certificate);
        }
    }
}
