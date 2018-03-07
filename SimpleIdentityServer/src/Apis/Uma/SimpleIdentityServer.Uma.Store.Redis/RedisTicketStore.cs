using SimpleIdentityServer.Uma.Core.Models;
using SimpleIdentityServer.Uma.Store.Redis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Uma.Core.Stores
{
    internal sealed class RedisTicketStore : ITicketStore
    {
        private readonly RedisStorage _storage;

        public RedisTicketStore(RedisStorage redisStorage)
        {
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

        public async Task<bool> AddAsync(Ticket ticket)
        {
            if (ticket == null)
            {
                throw new ArgumentNullException(nameof(ticket));
            }

            var record = await _storage.TryGetValueAsync<Ticket>(ticket.Id);
            if (record != null)
            {
                return false;
            }

            await _storage.SetAsync(ticket.Id, ticket, ticket.ExpiresIn);
            return true;
        }

        public async Task<bool> RemoveAsync(string ticketId)
        {
            if (string.IsNullOrWhiteSpace(ticketId))
            {
                throw new ArgumentNullException(nameof(ticketId));
            }

            var record = await _storage.TryGetValueAsync<Ticket>(ticketId);
            if (record == null)
            {
                return false;
            }

            await _storage.RemoveAsync(ticketId);
            return true;
        }

        public async Task<Ticket> GetAsync(string ticketId)
        {
            if (string.IsNullOrWhiteSpace(ticketId))
            {
                throw new ArgumentNullException(nameof(ticketId));
            }

            return await _storage.TryGetValueAsync<Ticket>(ticketId);
        }
    }
}
