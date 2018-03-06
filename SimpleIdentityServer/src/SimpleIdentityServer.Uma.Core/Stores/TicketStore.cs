using SimpleIdentityServer.Uma.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Uma.Core.Stores
{
    public interface ITicketStore
    {
        Task<bool> AddAsync(IEnumerable<Ticket> tickets);
        Task<bool> AddAsync(Ticket ticket);
        Task<bool> RemoveAsync(string ticketId);
        Task<Ticket> GetAsync(string ticketId);
    }

    internal sealed class InMemoryTicketStore : ITicketStore
    {
        private Dictionary<string, Ticket> _mappingStrToTickets;

        public InMemoryTicketStore()
        {
            _mappingStrToTickets = new Dictionary<string, Ticket>();
        }

        public async Task<bool> AddAsync(IEnumerable<Ticket> tickets)
        {
            if (tickets == null)
            {
                throw new ArgumentNullException(nameof(tickets));
            }

            foreach(var ticket in tickets)
            {
                if (!await AddAsync(ticket))
                {
                    return false;
                }
            }

            return true;
        }

        public Task<bool> AddAsync(Ticket ticket)
        {
            if (ticket == null)
            {
                throw new ArgumentNullException(nameof(ticket));
            }

            if (_mappingStrToTickets.ContainsKey(ticket.Id))
            {
                return Task.FromResult(false);
            }

            _mappingStrToTickets.Add(ticket.Id, ticket);
            return Task.FromResult(true);
        }

        public Task<bool> RemoveAsync(string ticketId)
        {
            if (string.IsNullOrWhiteSpace(ticketId))
            {
                throw new ArgumentNullException(nameof(ticketId));
            }

            if (!_mappingStrToTickets.ContainsKey(ticketId))
            {
                return Task.FromResult(false);
            }

            _mappingStrToTickets.Remove(ticketId);
            return Task.FromResult(true);
        }

        public Task<Ticket> GetAsync(string ticketId)
        {
            if (string.IsNullOrWhiteSpace(ticketId))
            {
                throw new ArgumentNullException(nameof(ticketId));
            }

            if (!_mappingStrToTickets.ContainsKey(ticketId))
            {
                return Task.FromResult((Ticket)null);
            }

            return Task.FromResult(_mappingStrToTickets[ticketId]);
        }
    }
}
