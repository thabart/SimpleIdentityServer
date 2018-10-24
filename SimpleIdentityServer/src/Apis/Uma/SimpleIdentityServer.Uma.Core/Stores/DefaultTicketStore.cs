using SimpleIdentityServer.Uma.Core.Models;
using SimpleIdServer.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Uma.Core.Stores
{
    internal sealed class DefaultTicketStore : ITicketStore
    {
        private static string TICKETS_CACHE_KEY = "uma_tickets";
        private static string TICKET_CACHE_KEY = "uma_ticket_{0}";
        public IStorage _storage;

        public DefaultTicketStore(IStorage storage)
        {
            _storage = storage;
        }

        public async Task<bool> AddAsync(IEnumerable<Ticket> tickets)
        {
            if (tickets == null)
            {
                throw new ArgumentNullException(nameof(tickets));
            }

            var ticketIds = await _storage.TryGetValueAsync<List<string>>(TICKETS_CACHE_KEY).ConfigureAwait(false);
            if (ticketIds == null)
            {
                ticketIds = new List<string>();
            }

            foreach(var ticket in tickets)
            {
                ticketIds.Add(ticket.Id);
                await _storage.SetAsync(string.Format(TICKET_CACHE_KEY, ticket.Id), ticket).ConfigureAwait(false);
            }

            await _storage.SetAsync(TICKETS_CACHE_KEY, ticketIds).ConfigureAwait(false);
            return true;
        }

        public Task<bool> AddAsync(Ticket ticket)
        {
            if (ticket == null)
            {
                throw new ArgumentNullException(nameof(ticket));
            }

            return AddAsync(new[] { ticket });
        }

        public Task<Ticket> GetAsync(string ticketId)
        {
            if (string.IsNullOrWhiteSpace(ticketId))
            {
                throw new ArgumentNullException(nameof(ticketId));
            }

            return _storage.TryGetValueAsync<Ticket>(string.Format(TICKET_CACHE_KEY, ticketId));
        }

        public async Task<bool> RemoveAsync(string ticketId)
        {
            if (string.IsNullOrWhiteSpace(ticketId))
            {
                throw new ArgumentNullException(nameof(ticketId));
            }

            var ticketIds = await _storage.TryGetValueAsync<List<string>>(TICKETS_CACHE_KEY).ConfigureAwait(false);
            if (ticketIds == null)
            {
                ticketIds = new List<string>();
            }

            var str = ticketIds.FirstOrDefault(tid => tid == ticketId);
            if (!string.IsNullOrWhiteSpace(str))
            {
                ticketIds.Remove(str);
                await _storage.SetAsync(TICKETS_CACHE_KEY, ticketIds).ConfigureAwait(false);
            }

            await _storage.RemoveAsync(string.Format(TICKET_CACHE_KEY, ticketId)).ConfigureAwait(false);
            return true;
        }
    }
}
