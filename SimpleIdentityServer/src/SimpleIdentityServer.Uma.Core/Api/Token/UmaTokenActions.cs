using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Uma.Core.Api.Token.Actions;
using SimpleIdentityServer.Uma.Core.Parameters;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Uma.Core.Api.Token
{
    public interface IUmaTokenActions
    {
        Task<GrantedToken> GetTokenByTicketId(GetTokenViaTicketIdParameter parameter, AuthenticationHeaderValue authenticationHeaderValue);
    }

    internal sealed class UmaTokenActions : IUmaTokenActions
    {
        private readonly IGetTokenByTicketIdAction _getTokenByTicketIdAction;

        public UmaTokenActions(IGetTokenByTicketIdAction getTokenByTicketIdAction)
        {
            _getTokenByTicketIdAction = getTokenByTicketIdAction;
        }

        public Task<GrantedToken> GetTokenByTicketId(GetTokenViaTicketIdParameter parameter, AuthenticationHeaderValue authenticationHeaderValue)
        {
            return _getTokenByTicketIdAction.Execute(parameter, authenticationHeaderValue);
        }
    }
}
