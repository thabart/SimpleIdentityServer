using Newtonsoft.Json;
using SimpleIdentityServer.Uma.Core.Errors;
using SimpleIdentityServer.Uma.Core.Exceptions;
using SimpleIdentityServer.Uma.Core.Parameters;
using SimpleIdentityServer.Uma.Core.Services;
using SimpleIdentityServer.Uma.Core.Stores;
using SimpleIdentityServer.Uma.Logging;
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Uma.Core.Api.Token.Actions
{
    public interface IGetTokenByTicketIdAction
    {

    }

    internal sealed class GetTokenByTicketIdAction : IGetTokenByTicketIdAction
    {
        private readonly ITicketStore _ticketStore;
        private readonly IConfigurationService _configurationService;
        private readonly IUmaServerEventSource _umaServerEventSource;

        public GetTokenByTicketIdAction(ITicketStore ticketStore, IConfigurationService configurationService, IUmaServerEventSource umaServerEventSource)
        {
            _ticketStore = ticketStore;
            _configurationService = configurationService;
            _umaServerEventSource = umaServerEventSource;
        }

        public async Task Execute(GetTokenViaTicketIdParameter parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            if (string.IsNullOrWhiteSpace(parameter.Ticket))
            {
                throw new ArgumentNullException(nameof(parameter.Ticket));
                // TODO : THROW AN EXCEPTION.
            }

            var ticket = await _ticketStore.GetAsync(parameter.Ticket);
            if (ticket == null)
            {
                // TODO: THROW AN EXCEPTION.
            }

            if (ticket.ExpirationDateTime < DateTime.UtcNow)
            {
                throw new BaseUmaException(ErrorCodes.ExpiredTicket, ErrorDescriptions.TheTicketIsExpired);
            }


            _umaServerEventSource.StartGettingAuthorization(JsonConvert.SerializeObject(parameter));
        }
    }
}
